using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Smart_teatcher_MVC.Models;
using System.Security.Claims;

namespace Smart_teatcher_MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly string apiBaseUrl = $"http://{connected_ip.connection}/api";
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync($"{apiBaseUrl}/Users/HasRoot");
                    var hasRoot = JsonConvert.DeserializeObject<bool>(await response.Content.ReadAsStringAsync());

                    if (hasRoot)
                    {
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        return RedirectToAction("Register");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطأ في التحقق من وجود مستخدم رئيسي");
                    return RedirectToAction("Login"); // في حالة الخطأ، نفترض وجود مستخدم رئيسي
                }
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.PostAsJsonAsync($"{apiBaseUrl}/Users/Login", model);
                    if (response.IsSuccessStatusCode)
                    {
                        var user = await response.Content.ReadFromJsonAsync<User>();

                        // إنشاء claims للمستخدم
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.Username),
                            new Claim(ClaimTypes.Role, user.Role)
                        };

                        // جلب صلاحيات المستخدم
                        var permissionsResponse = await client.GetAsync($"{apiBaseUrl}/Users/Permissions/{user.Id}");
                        if (permissionsResponse.IsSuccessStatusCode)
                        {
                            var permissions = await permissionsResponse.Content.ReadFromJsonAsync<UserPermissions>();
                            if (permissions != null)
                            {
                                // إضافة الصلاحيات إلى claims
                                if (permissions.CanViewSentences) claims.Add(new Claim("CanViewSentences", "true"));
                                if (permissions.CanAddSentences) claims.Add(new Claim("CanAddSentences", "true"));
                                if (permissions.CanEditSentences) claims.Add(new Claim("CanEditSentences", "true"));
                                if (permissions.CanDeleteSentences) claims.Add(new Claim("CanDeleteSentences", "true"));
                                if (permissions.CanViewWords) claims.Add(new Claim("CanViewWords", "true"));
                                if (permissions.CanAddWords) claims.Add(new Claim("CanAddWords", "true"));
                                if (permissions.CanEditWords) claims.Add(new Claim("CanEditWords", "true"));
                                if (permissions.CanDeleteWords) claims.Add(new Claim("CanDeleteWords", "true"));
                                if (permissions.CanViewMathOperations) claims.Add(new Claim("CanViewMathOperations", "true"));
                                if (permissions.CanAddMathOperations) claims.Add(new Claim("CanAddMathOperations", "true"));
                                if (permissions.CanEditMathOperations) claims.Add(new Claim("CanEditMathOperations", "true"));
                                if (permissions.CanDeleteMathOperations) claims.Add(new Claim("CanDeleteMathOperations", "true"));
                            }
                        }

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        // تخزين معلومات المستخدم في الجلسة
                        HttpContext.Session.SetString("UserId", user.Id.ToString());
                        HttpContext.Session.SetString("Username", user.Username);
                        HttpContext.Session.SetString("UserRole", user.Role);

                        TempData["Success"] = $"مرحباً {user.Username}!";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            ModelState.AddModelError("", "اسم المستخدم أو كلمة المرور غير صحيحة");
                        }
                        else
                        {
                            ModelState.AddModelError("", error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطأ في تسجيل الدخول");
                    ModelState.AddModelError("", "حدث خطأ أثناء محاولة تسجيل الدخول");
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            // التحقق من وجود مستخدم root قبل عرض صفحة التسجيل
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync($"{apiBaseUrl}/Users/HasRoot");
                    var hasRoot = JsonConvert.DeserializeObject<bool>(await response.Content.ReadAsStringAsync());

                    if (hasRoot)
                    {
                        return RedirectToAction("Login");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطأ في التحقق من وجود مستخدم رئيسي");
                }
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var client = new HttpClient())
            {
                try
                {
                    // إنشاء نموذج API مع البريد الإلكتروني
                    var apiModel = new
                    {
                        Username = model.Username,
                        Password = model.Password,
                        Email = model.Email
                    };

                    var response = await client.PostAsJsonAsync($"{apiBaseUrl}/Users/Register", apiModel);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "تم إنشاء الحساب بنجاح";
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("", error);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطأ في إنشاء الحساب");
                    ModelState.AddModelError("", "حدث خطأ أثناء محاولة إنشاء الحساب");
                }
            }

            return View(model);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
