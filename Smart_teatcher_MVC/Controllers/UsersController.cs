using Microsoft.AspNetCore.Mvc;
using Smart_teatcher_MVC.Models;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;

namespace Smart_teatcher_MVC.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly ILogger<UsersController> _logger;
        private readonly string apiBaseUrl = $"http://{connected_ip.connection}/api";

        public UsersController(ILogger<UsersController> logger)
        {
            _logger = logger;
        }

        // صفحة إدارة المستخدمين
        public async Task<IActionResult> Index()
        {
            if (!User.IsInRole("Root"))
            {
                return RedirectToAction("Index", "Home");
            }

            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync($"{apiBaseUrl}/Users");
                    if (response.IsSuccessStatusCode)
                    {
                        var users = await response.Content.ReadFromJsonAsync<List<User>>();
                        return View(users);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطأ في جلب قائمة المستخدمين");
                    TempData["Error"] = "حدث خطأ أثناء جلب قائمة المستخدمين";
                }
            }

            return View(new List<User>());
        }

        // تفعيل/تعطيل مستخدم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            if (!User.IsInRole("Root"))
            {
                return Json(new { success = false, message = "غير مصرح لك بهذه العملية" });
            }

            using (var client = new HttpClient())
            {
                try
                {
                    // جلب حالة المستخدم الحالية
                    var userResponse = await client.GetAsync($"{apiBaseUrl}/Users/{id}");
                    if (!userResponse.IsSuccessStatusCode)
                    {
                        return Json(new { success = false, message = "لم يتم العثور على المستخدم" });
                    }

                    var user = await userResponse.Content.ReadFromJsonAsync<User>();
                    var isCurrentlyActive = user.IsActive;

                    // تغيير الحالة
                    var response = await client.PostAsync($"{apiBaseUrl}/Users/ToggleActive/{id}", null);
                    if (response.IsSuccessStatusCode)
                    {
                        var message = isCurrentlyActive 
                            ? "تم تعطيل المستخدم بنجاح" 
                            : "تم تفعيل المستخدم بنجاح";
                        return Json(new { success = true, message = message });
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = error });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطأ في تغيير حالة المستخدم");
                    return Json(new { success = false, message = "حدث خطأ أثناء تغيير حالة المستخدم" });
                }
            }
        }

        // إضافة مستخدم جديد
        [HttpGet]
        public IActionResult Create()
        {
            if (!User.IsInRole("Root"))
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (!User.IsInRole("Root"))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var client = new HttpClient())
            {
                try
                {
                    var apiModel = new
                    {
                        Username = model.Username,
                        Password = model.Password,
                        Email = model.Email,
                        Role = "Admin" // دائماً نضيف كأدمن فرعي
                    };

                    var response = await client.PostAsJsonAsync($"{apiBaseUrl}/Users/Register", apiModel);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "تم إضافة المستخدم بنجاح";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("", error);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطأ في إضافة مستخدم جديد");
                    ModelState.AddModelError("", "حدث خطأ أثناء محاولة إضافة المستخدم");
                }
            }

            return View(model);
        }

        // تعديل مستخدم
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!User.IsInRole("Root"))
            {
                return RedirectToAction("Index", "Home");
            }

            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync($"{apiBaseUrl}/Users/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        var user = await response.Content.ReadFromJsonAsync<User>();
                        var model = new RegisterViewModel
                        {
                            Username = user.Username,
                            Email = user.Email
                        };
                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطأ في جلب بيانات المستخدم");
                    TempData["Error"] = "حدث خطأ أثناء جلب بيانات المستخدم";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RegisterViewModel model)
        {
            if (!User.IsInRole("Root"))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var client = new HttpClient())
            {
                try
                {
                    var apiModel = new
                    {
                        Username = model.Username,
                        Email = model.Email,
                        Password = model.Password
                    };

                    var response = await client.PutAsJsonAsync($"{apiBaseUrl}/Users/Update/{id}", apiModel);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "تم تحديث بيانات المستخدم بنجاح";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("", error);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطأ في تحديث بيانات المستخدم");
                    ModelState.AddModelError("", "حدث خطأ أثناء محاولة تحديث بيانات المستخدم");
                }
            }

            return View(model);
        }

        // حذف مستخدم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.IsInRole("Root"))
            {
                return Json(new { success = false, message = "غير مصرح لك بهذه العملية" });
            }

            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.DeleteAsync($"{apiBaseUrl}/Users/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true, message = "تم حذف المستخدم بنجاح" });
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = error });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطأ في حذف المستخدم");
                    return Json(new { success = false, message = "حدث خطأ أثناء محاولة حذف المستخدم" });
                }
            }
        }

        // صفحة صلاحيات المستخدم
        [HttpGet]
        public async Task<IActionResult> Permissions(int id)
        {
            if (!User.IsInRole("Root"))
            {
                return RedirectToAction("Index", "Home");
            }

            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync($"{apiBaseUrl}/Users/Permissions/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        var permissions = await response.Content.ReadFromJsonAsync<UserPermissions>();
                        ViewBag.UserId = id;
                        return View(permissions);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطأ في جلب صلاحيات المستخدم");
                    TempData["Error"] = "حدث خطأ أثناء جلب صلاحيات المستخدم";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePermissions(int id, UserPermissions permissions)
        {
            if (!User.IsInRole("Root"))
            {
                return Json(new { success = false, message = "غير مصرح لك بهذه العملية" });
            }

            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.PutAsJsonAsync($"{apiBaseUrl}/Users/Permissions/{id}", permissions);
                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true, message = "تم تحديث الصلاحيات بنجاح" });
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = error });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطأ في تحديث صلاحيات المستخدم");
                    return Json(new { success = false, message = "حدث خطأ أثناء تحديث الصلاحيات" });
                }
            }
        }
    }
}
