using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using platform.Models;
using platform.Models.Account;
using platform.Models.Coaches;
using platform.Models.Courses;

namespace platform.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly PlatformDbContext _context;
        private readonly ILogger _logger;

        public AdminController(
            PlatformDbContext context,
            UserManager<User> userManager,
            ILogger<AdminController> logger
        )
        {
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ValidateCourses()
        {
            var roleClaims = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
            if (!roleClaims.Contains("Admin"))
            {
                // TODO: implement forbid
                return Forbid();
            }
            var allPendingCourses = _context.PendingCourses;
            return View(allPendingCourses);
        }

        [HttpGet]
        public async Task<IActionResult> ValidateCoaches()
        {
            var roleClaims = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
            if (!roleClaims.Contains("Admin"))
            {
                // TODO: implement forbid
                return Forbid();
            }
            var allPendingCoaches = _context.PendingCoaches;
            return View(allPendingCoaches);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveCourse(int id)
        {
            var pendingCourse = await _context.PendingCourses.FindAsync(id);
            if (pendingCourse == null)
            {
                _logger.Log(
                    LogLevel.Error,
                    $"[AdminController - ApproveCourse] Course with ID {id} not found."
                );
                return NotFound();
            }

            var newCourse = new Course()
            {
                Title = pendingCourse.Title,
                Price = pendingCourse.Price,
                Path = pendingCourse.Path,
                Description = pendingCourse.Description,
                UserId = pendingCourse.UserId,
                Genre = pendingCourse.Genre,
            };

            _context.PendingCourses.Remove(pendingCourse);
            _context.Courses.Add(newCourse);

            await _context.SaveChangesAsync();

            _logger.Log(
                LogLevel.Information,
                $"[AdminController - ApproveCourse] Course {pendingCourse.Title} approved."
            );
            return RedirectToAction("ValidateCourses");
        }

        [HttpPost]
        public async Task<IActionResult> RejectCourse(int id)
        {
            var course = await _context.PendingCourses.FindAsync(id);
            if (course == null)
            {
                _logger.Log(
                    LogLevel.Error,
                    $"[AdminController - RejectCourse] Course with ID {id} not found."
                );
                return NotFound();
            }

            _context.PendingCourses.Remove(course);

            await _context.SaveChangesAsync();

            _logger.Log(
                LogLevel.Information,
                $"[AdminController - RejectCourse] Course {course.Title} rejected."
            );
            return RedirectToAction("ValidateCourses");
        }

        [HttpPost]
        public async Task<IActionResult> ApproveCoach(int id)
        {
            var pendingCoach = await _context.PendingCoaches.FindAsync(id);
            if (pendingCoach == null)
            {
                _logger.Log(
                    LogLevel.Error,
                    $"[AdminController - ApproveCoach] Coach with ID {id} not found."
                );
                return NotFound();
            }

            var newCoach = new Coach()
            {
                PhoneNumber = pendingCoach.PhoneNumber,
                PicturePath = pendingCoach.PicturePath,
                Bio = pendingCoach.Bio,
                UserId = pendingCoach.UserId,
                City = pendingCoach.City,
                Level = pendingCoach.Level,
                ExpertiseArea = pendingCoach.ExpertiseArea,
            };

            _context.PendingCoaches.Remove(pendingCoach);
            _context.Coaches.Add(newCoach);

            await _context.SaveChangesAsync();

            //            _logger.Log(
            //                LogLevel.Information,
            //                $"[AdminController - ApproveCoach] Coach {pendingCoach.User.UserName} approved."
            //            );
            return RedirectToAction("ValidateCoaches");
        }

        [HttpPost]
        public async Task<IActionResult> RejectCoach(int id)
        {
            var pendingCoach = await _context.PendingCoaches.FindAsync(id);
            if (pendingCoach == null)
            {
                _logger.Log(
                    LogLevel.Error,
                    $"[AdminController - RejectCoach] PendingCoach with ID {id} not found."
                );
                return NotFound();
            }

            _context.PendingCoaches.Remove(pendingCoach);

            await _context.SaveChangesAsync();

            _logger.Log(
                LogLevel.Information,
                $"[AdminController - RejectCoach] Coach {pendingCoach.User.UserName} rejected."
            );
            return RedirectToAction("ValidateCoaches");
        }
    }
}
