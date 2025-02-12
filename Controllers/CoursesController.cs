using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using platform.Models;
using platform.Models.Account;
using platform.Models.Courses;

namespace platform.Controllers
{
    public class CoursesController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger _logger;
        private readonly PlatformDbContext _context;

        public CoursesController(
            UserManager<User> userManager,
            ILogger<CoursesController> logger,
            PlatformDbContext context
        )
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int? minPrice,
            int? maxPrice,
            string? genre,
            string? sortOrder
        )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // fetch courses excluding user's own and purchased courses
            var purchasedCourseIds = _context
                .Purchases.Where(p => p.UserId == userId)
                .Select(p => p.CourseId);

            var courses = _context
                .Courses.Where(c => c.UserId != userId && !purchasedCourseIds.Contains(c.CourseId))
                .AsQueryable();

            if (minPrice.HasValue)
            {
                if (minPrice > 1e3)
                {
                    ModelState.AddModelError("MinPrice", "The minimum price cannot be over 1000!");
                }
                else
                    courses = courses.Where(c => c.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                if (maxPrice < 0)
                {
                    ModelState.AddModelError("MaxPrice", "The maximum price cannot be negative!");
                }
                else
                    courses = courses.Where(c => c.Price <= maxPrice.Value);
            }

            if (!string.IsNullOrEmpty(genre))
            {
                if (
                    Enum.TryParse(typeof(platform.Models.Courses.Genre), genre, out var parsedGenre)
                )
                {
                    courses = courses.Where(c =>
                        c.Genre == (platform.Models.Courses.Genre)parsedGenre
                    );
                }
            }

            courses =
                sortOrder == "price_desc"
                    ? courses.OrderByDescending(c => c.Price)
                    : courses.OrderBy(c => c.Price);

            return View(courses);
        }

        [HttpGet]
        public IActionResult AddCourse()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCourse(CourseAddForm model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                ModelState.AddModelError("Title", "The course title is required.");
                return View();
            }
            else if (model.Title.Length > 100)
            {
                ModelState.AddModelError("Title", "The course title cannot exceed 100 characters.");
            }

            if (model.Price <= 0)
            {
                ModelState.AddModelError("Price", "The course price must be a positive value.");
            }
            else if (model.Price > 1000)
            {
                ModelState.AddModelError("Price", "The course price cannot exceed 1000.");
            }

            if (string.IsNullOrWhiteSpace(model.Description))
            {
                ModelState.AddModelError("Description", "A course description is required.");
            }
            else if (model.Description.Length < 20)
            {
                ModelState.AddModelError(
                    "Description",
                    "The course description must be at least 20 characters long."
                );
            }

            if (!Enum.IsDefined(typeof(platform.Models.Courses.Genre), model.Genre))
            {
                ModelState.AddModelError("Genre", "The selected genre is invalid.");
            }

            if (model.File == null)
            {
                ModelState.AddModelError("File", "Please upload a course file.");
            }
            else if (model.File.Length == 0)
            {
                ModelState.AddModelError("File", "The uploaded file cannot be empty.");
            }
            else if (
                !Path.GetExtension(model.File.FileName)
                    .Equals(".pdf", StringComparison.OrdinalIgnoreCase)
            )
            {
                ModelState.AddModelError("File", "Only PDF files are allowed.");
            }

            if (!ModelState.IsValid)
            {
                _logger.Log(
                    LogLevel.Warning,
                    "[CourseController - AddCourse - POST] Model state is invalid"
                );
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.Log(LogLevel.Error, "[CourseController - AddCourse - POST] User not found");
                return RedirectToAction("Login", "Account");
            }

            var pendingCourse = new PendingCourse
            {
                Title = model.Title,
                Price = model.Price,
                Description = model.Description,
                UserId = user.Id,
                Genre = model.Genre,
            };

            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "courses");

            if (!Directory.Exists(directoryPath))
            {
                _logger.Log(
                    LogLevel.Information,
                    "[CourseController - AddCourse - post] Directory does not exist. Creating directory..."
                );
                Directory.CreateDirectory(directoryPath);
            }

            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var fileHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(model.File.FileName));
                var hashedFileName =
                    BitConverter.ToString(fileHash).Replace("-", "").ToLower()
                    + Path.GetExtension(model.File.FileName);
                var filePath = Path.Combine(directoryPath, hashedFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(fileStream);
                }

                pendingCourse.Path = Path.Combine("courses", hashedFileName);

                _context.PendingCourses.Add(pendingCourse);
                await _context.SaveChangesAsync();

                ViewBag.Message =
                    "Courses sent to verification, you will receive an email when the course has been added";

                _logger.Log(
                    LogLevel.Information,
                    "[CourseController - AddCourse - POST] Pending Course added successfully"
                );
            }

            return RedirectToAction("Index", "Courses");
        }

        [HttpGet]
        public async Task<IActionResult> ViewDetails(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            course.User = await _context.Users.FindAsync(course.UserId);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        [HttpPost]
        public async Task<IActionResult> Purchase(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);

            if (course == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("You must be logged in to purchase a course.");
            }

            var purchase = new Purchase
            {
                CourseId = courseId,
                UserId = userId,
                PurchaseDate = DateTime.Now,
                AmountPaid = course.Price,
            };

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Purchase successful!";
            return RedirectToAction("Confirmation", new { id = courseId });
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        [HttpGet]
        public IActionResult DashboardCourses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("You must be logged in to view your dashboard.");
            }

            var boughtCoursesIds = _context
                .Purchases.Where(p => p.UserId == userId)
                .Select(p => p.CourseId)
                .ToList();

            var boughtCourses = _context
                .Courses.Where(c => boughtCoursesIds.Contains(c.CourseId))
                .ToList();

            var ownedCourses = _context.Courses.Where(c => c.UserId == userId).ToList();

            var dashboardModel = new DashboardCoursesViewModel
            {
                BoughtCourses = boughtCourses,
                OwnedCourses = ownedCourses,
            };

            return View(dashboardModel);
        }
    }
}
