using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using platform.Models;
using platform.Models.Account;
using platform.Services.Email;

namespace platform.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly PlatformDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger _logger;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger,
            IEmailService emailService,
            PlatformDbContext context
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _emailService = emailService;
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginForm model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.Message = "Password does not match the confirm password";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ViewBag.Message = "A user does not exist with the specified email";
                return View();
            }

            //if (!user.EmailConfirmed)
            //{
            //    ViewBag.Message =
            //        "The account is not verified, check the verification on your email!";
            //    return View();
            //}

            var check = await _userManager.CheckPasswordAsync(user, model.Password);
            if (check == false)
            {
                ViewBag.Message = "Wrong password";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            _logger.Log(
                LogLevel.Information,
                $"[AccountController - Login - get] Roles: {roles[0]}"
            );

            // set up the cookies
            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );
            var principal = new ClaimsPrincipal(identity);

            //            var resultRole = await _userManager.AddToRoleAsync(user, roles[0]);
            //
            //            if (!resultRole.Succeeded)
            //            {
            //                ModelState.AddModelError(string.Empty, "Invalid login attempt role.");
            //                _logger.Log(
            //                    LogLevel.Information,
            //                    "[AccountController - Login ] Error on atributing the role"
            //                );
            //                return View();
            //            }

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                true,
                false
            );

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View();
            }

            _logger.Log(
                LogLevel.Information,
                "[AccountController - Login - post] a trecut de login"
            );

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegitrationForm model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.Message = "Password does not match the confirm password";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                ViewBag.Message = "User already exists with this email";
                return View();
            }

            User userToRegister = new User()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                ActivationCode = Guid.NewGuid(),
                ResetPasswordCode = Guid.NewGuid(),
                EmailConfirmed = false,
                UserName = model.UserName,
                Email = model.Email,
            };

            var result = await _userManager.CreateAsync(userToRegister, model.Password);

            if (result.Succeeded)
            {
                var role = "USER"; // HEREROLE

                var roleExists = await _roleManager.RoleExistsAsync(role);
                if (!roleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                await _userManager.AddToRoleAsync(userToRegister, role);

                _emailService.SendEmailYahooRegister(
                    userToRegister.Email,
                    userToRegister.ActivationCode
                );
                ViewBag.Message = "Registration confirmed, activation code sent to email";

                _logger.Log(
                    LogLevel.Information,
                    $"[AccountController - Register] Email sent to: {userToRegister.Email}"
                );
            }
            else
            {
                var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
                ViewBag.Message = "Registration not confirmed." + errorMessages;

                _logger.LogError(
                    "User registration failed for email: {Email}. Errors: {Errors}",
                    model.Email,
                    errorMessages
                );
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> VerifyAccount(Guid activationCode)
        {
            if (activationCode == Guid.Empty)
            {
                ViewBag.Message = "Invalid activation code.";
                return View("VerifyAccount");
            }

            var user = _context.Users.FirstOrDefault(u => u.ActivationCode == activationCode);

            if (user == null)
            {
                ViewBag.Message = "Invalid code or expired.";
                return View("VerifyAccount");
            }

            if (user.EmailConfirmed)
            {
                ViewBag.Message = "Account is already verified.";
                return View("VerifyAccount");
            }

            user.EmailConfirmed = true;
            user.ActivationCode = Guid.NewGuid();

            await _userManager.UpdateAsync(user);

            ViewBag.Message = "Account successfully verified. You can now log in.";
            return View("VerifyAccount");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordForm model)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                ViewBag.Message = "No user has the given email";
                return View(); // it goes back to ForgotPassword - get
            }
            ViewBag.Message = "A verification code has been sent";
            _emailService.SendEmailYahooForgot(model.Email, user.ResetPasswordCode);

            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(Guid resetPasswordCode)
        {
            if (resetPasswordCode == Guid.Empty)
            {
                ViewBag.Message = "Invalid reset password code.";
                return View("ForgotPassword");
            }
            var user = _context.Users.FirstOrDefault(u => u.ResetPasswordCode == resetPasswordCode);
            var resetModel = new ResetPasswordForm()
            {
                ResetPasswordCode = resetPasswordCode,
                Email = user.Email,
            };
            return View(resetModel);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordPost(ResetPasswordForm model)
        {
            if (model.ResetPasswordCode == Guid.Empty)
            {
                ViewBag.Message = "Invalid reset password code.";
                return View("ForgotPassword");
            }

            var user = _context.Users.FirstOrDefault(u =>
                u.ResetPasswordCode == model.ResetPasswordCode
            );
            if (user == null)
            {
                ViewBag.Message = "Invalid or expired reset password code.";
                return View("ForgotPassword");
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                ViewBag.Message = "Passwords do not match.";
                return View();
            }

            var result = await _userManager.RemovePasswordAsync(user);
            if (!result.Succeeded)
            {
                ViewBag.Message = "Error removing the old password.";
                return View();
            }

            result = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (!result.Succeeded)
            {
                var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
                ViewBag.Message = $"Error setting new password: {errorMessages}";
                return View();
            }

            user.ResetPasswordCode = Guid.NewGuid();
            await _userManager.UpdateAsync(user);

            ViewBag.Message = "Password successfully reset. You can now log in.";
            return RedirectToAction("Login");
        }
    }
}
