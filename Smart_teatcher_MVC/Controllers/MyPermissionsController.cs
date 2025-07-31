using Microsoft.AspNetCore.Mvc;
using Smart_teatcher_MVC.Models;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Smart_teatcher_MVC.Controllers
{
    [Authorize]
    public class MyPermissionsController : Controller
    {
        private readonly string _apiBaseUrl = $"http://{connected_ip.connection}/api";
        private readonly ILogger<MyPermissionsController> _logger;

        public MyPermissionsController(ILogger<MyPermissionsController> logger)
        {
            _logger = logger;
        }

        private UserPermissions GetRootPermissions()
        {
            return new UserPermissions
            {
                // صلاحيات الجمل
                CanViewSentences = true,
                CanAddSentences = true,
                CanEditSentences = true,
                CanDeleteSentences = true,

                // صلاحيات الكلمات
                CanViewWords = true,
                CanAddWords = true,
                CanEditWords = true,
                CanDeleteWords = true,

                // صلاحيات العمليات الحسابية
                CanViewMathOperations = true,
                CanAddMathOperations = true,
                CanEditMathOperations = true,
                CanDeleteMathOperations = true
            };
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "لم يتم العثور على معرف المستخدم";
                    return RedirectToAction("Index", "Home");
                }

                using (var client = new HttpClient())
                {
                    // Get user details first
                    var userResponse = await client.GetAsync($"{_apiBaseUrl}/Users/{userId}");
                    if (!userResponse.IsSuccessStatusCode)
                    {
                        TempData["Error"] = "حدث خطأ أثناء جلب بيانات المستخدم";
                        return RedirectToAction("Index", "Home");
                    }

                    var user = await userResponse.Content.ReadFromJsonAsync<User>();
                    if (user == null)
                    {
                        TempData["Error"] = "لم يتم العثور على بيانات المستخدم";
                        return RedirectToAction("Index", "Home");
                    }

                    ViewBag.UserName = user.Username;
                    ViewBag.Email = user.Email;
                    ViewBag.IsRoot = user.Role == "Root";
                    ViewBag.IsAdmin = user.Role == "Admin";

                    // إذا كان المستخدم Root، نعيد جميع الصلاحيات true
                    if (user.Role == "Root")
                    {
                        return View(GetRootPermissions());
                    }

                    // إذا لم يكن Root، نجلب الصلاحيات من API
                    var response = await client.GetAsync($"{_apiBaseUrl}/Users/Permissions/{userId}");
                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["Error"] = "حدث خطأ أثناء جلب الصلاحيات";
                        return RedirectToAction("Index", "Home");
                    }

                    var permissions = await response.Content.ReadFromJsonAsync<UserPermissions>();
                    if (permissions == null)
                    {
                        TempData["Error"] = "لم يتم العثور على صلاحيات";
                        return RedirectToAction("Index", "Home");
                    }

                    return View(permissions);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user permissions");
                TempData["Error"] = "حدث خطأ أثناء جلب الصلاحيات";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
