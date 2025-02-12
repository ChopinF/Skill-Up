using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using platform.Models;
using platform.Models.Account;
using platform.Models.Coaches;

namespace platform.Controllers
{
    public class CoachesController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger _logger;
        private readonly PlatformDbContext _context;

        public CoachesController(
            UserManager<User> userManager,
            ILogger<CoachesController> logger,
            PlatformDbContext context
        )
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string city, string level, string expertise)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            TempData["IsCoach"] = _context.Coaches.Any(c => c.UserId == userId);

            var query = _context.Coaches.AsQueryable();

            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(coach => coach.City.ToString() == city);
            }

            if (!string.IsNullOrEmpty(level))
            {
                query = query.Where(coach => coach.Level.ToString() == level);
            }

            if (!string.IsNullOrEmpty(expertise))
            {
                query = query.Where(coach => coach.ExpertiseArea.ToString() == expertise);
            }

            var allCoaches = await query
                .Where(c => c.UserId != userId)
                .Include(coach => coach.User)
                .ToListAsync();

            return View(allCoaches);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var coach = await _context
                .Coaches.Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CoachId == id);

            if (coach == null)
            {
                return NotFound();
            }

            return View(coach);
        }

        [HttpGet]
        public IActionResult BookSession(int coachId)
        {
            _logger.Log(
                LogLevel.Information,
                $"[CoachesController - BookSession - get] A intrat {coachId}"
            );
            var coach = _context.Coaches.Find(coachId);
            if (coach == null)
                return NotFound();

            return View(coach);
        }

        [HttpPost]
        public async Task<IActionResult> BookSession(Coach coach, DateTime date)
        {
            var booking = new Booking()
            {
                IsConfirmed = false,
                Date = date,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                CoachId = coach.CoachId,
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            ViewBag.Message = "The booking has been sent to the Coach, waiting to be confirmed";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult BecomeCoach()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BecomeCoach(BecomeCoachForm model)
        {
            if (String.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "The phone number should not be empty!");
                return View();
            }
            if (String.IsNullOrWhiteSpace(model.Bio))
            {
                ModelState.AddModelError("Bio", "The bio should not be empty!");
                return View();
            }
            if (model.Bio.Length > 100)
            {
                ModelState.AddModelError("Bio", "The bio should not be longer than 100 chracters!");
                return View();
            }

            if (String.IsNullOrWhiteSpace(model.City.ToString()))
            {
                ModelState.AddModelError("City", "You must choose a city");
                return View();
            }
            if (String.IsNullOrWhiteSpace(model.Level.ToString()))
            {
                ModelState.AddModelError("Level", "You must choose a level");
                return View();
            }
            if (String.IsNullOrWhiteSpace(model.ExpertiseArea.ToString()))
            {
                ModelState.AddModelError("ExpertiseArea", "You must choose a expertise area");
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found");
                return View();
            }

            string picturePath = null;
            if (model.PicturePath == null || model.PicturePath.Length == 0)
            {
                if (model.PicturePath == null)
                {
                    _logger.Log(LogLevel.Information, "Null");
                }
                if (model.PicturePath.Length == 0)
                {
                    _logger.Log(LogLevel.Information, "Is 0");
                }
                _logger.Log(LogLevel.Information, "No file uploaded or file is empty.");
                ModelState.AddModelError("PicturePath", "No file uploaded or file is empty.");
                return RedirectToAction("Index");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(model.PicturePath.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError(
                    "PicturePath",
                    "Invalid file type. Only images with the format: .jpg / .jpeg / .png are allowed."
                );
                return View();
            }

            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "coaches");

            if (!Directory.Exists(directoryPath))
            {
                _logger.Log(
                    LogLevel.Information,
                    "Directory does not exist. Creating directory..."
                );
                Directory.CreateDirectory(directoryPath);
            }

            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var fileHash = sha256.ComputeHash(
                    Encoding.UTF8.GetBytes(model.PicturePath.FileName)
                );
                var hashedFileName =
                    BitConverter.ToString(fileHash).Replace("-", "").ToLower()
                    + Path.GetExtension(model.PicturePath.FileName);

                var filePath = Path.Combine(directoryPath, hashedFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.PicturePath.CopyToAsync(fileStream);
                }

                picturePath = Path.Combine("coaches", hashedFileName);
            }

            var pendingCoach = new PendingCoach
            {
                PhoneNumber = model.PhoneNumber,
                Bio = model.Bio,
                City = model.City,
                Level = model.Level,
                ExpertiseArea = model.ExpertiseArea,
                UserId = user.Id,
                PicturePath = picturePath, // Save the file path
            };

            _context.PendingCoaches.Add(pendingCoach);
            await _context.SaveChangesAsync();

            ViewBag.Message =
                "Your request to become a coach has been sent for verification. You will receive an email if confirmed.";
            _logger.Log(LogLevel.Information, "BecomeCoach method completed successfully.");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult DashboardCoaches()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bookings = _context.Bookings.Where(c => c.UserId != userId).Include(b => b.User);

            _logger.Log(
                LogLevel.Information,
                $"[CoachesController - DashboardCoaches - get] Numbers : {bookings.Count()}"
            );

            //var bookings = _context.Bookings;
            return View(bookings);
        }

        [HttpGet]
        public IActionResult DashboardUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                _logger.Log(LogLevel.Warning, "User not found.");
                return Unauthorized();
            }

            // Retrieve pending bookings for the user
            var pendingBookings = _context
                .Bookings.Where(b => b.UserId == userId && !b.IsConfirmed)
                .Include(b => b.Coach)
                .Include(b => b.User)
                .ToList();

            _logger.Log(
                LogLevel.Information,
                $"[DashboardUser - GET] Found {pendingBookings.Count} pending bookings for user {userId}"
            );

            return View(pendingBookings);
        }

        [HttpPost]
        public async Task<IActionResult> DashboardCoachesApprove(string userId, int coachId)
        {
            var pendingBooking = await _context.Bookings.FirstOrDefaultAsync(b =>
                b.UserId == userId && b.CoachId == coachId
            );

            if (pendingBooking == null)
            {
                _logger.Log(
                    LogLevel.Warning,
                    $"Booking not found for user {userId} and coach {coachId}"
                );
                return NotFound("Booking not found.");
            }

            pendingBooking.IsConfirmed = true;
            _context.Bookings.Update(pendingBooking);
            await _context.SaveChangesAsync();

            _logger.Log(
                LogLevel.Information,
                $"Booking approved for user {userId} and coach {coachId}"
            );

            return RedirectToAction("DashboardCoaches");
        }

        [HttpPost]
        public async Task<IActionResult> DashboardCoachesReject(string userId, int coachId)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b =>
                b.UserId == userId && b.CoachId == coachId
            );

            if (booking == null)
            {
                _logger.Log(
                    LogLevel.Warning,
                    $"Booking not found for user {userId} and coach {coachId}"
                );
                return NotFound("Booking not found.");
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            _logger.Log(
                LogLevel.Information,
                $"Booking rejected for user {userId} and coach {coachId}"
            );

            return RedirectToAction("DashboardCoaches");
        }
    }
}
