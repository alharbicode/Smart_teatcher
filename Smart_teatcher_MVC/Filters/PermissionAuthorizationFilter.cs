using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Smart_teatcher_MVC.Models;
using System.Net.Http.Json;

namespace Smart_teatcher_MVC.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequirePermissionAttribute : TypeFilterAttribute
    {
        public RequirePermissionAttribute(string permission) 
            : base(typeof(PermissionAuthorizationFilter))
        {
            Arguments = new object[] { permission };
        }
    }

    public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly string _permission;
        private readonly ILogger<PermissionAuthorizationFilter> _logger;
        private readonly string _apiBaseUrl = $"http://{connected_ip.connection}/api";

        public PermissionAuthorizationFilter(string permission, ILogger<PermissionAuthorizationFilter> logger)
        {
            _permission = permission;
            _logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            try
            {
                if (!context.HttpContext.User.Identity.IsAuthenticated)
                {
                    context.Result = new RedirectToActionResult("Login", "Account", null);
                    return;
                }

                // إذا كان المستخدم Root، نسمح له بالوصول دائماً
                if (context.HttpContext.User.IsInRole("Root"))
                {
                    return;
                }

                var userId = context.HttpContext.Session.GetInt32("UserId");
                if (!userId.HasValue)
                {
                    _logger.LogWarning("UserId not found in session");
                    context.Result = new RedirectToActionResult("Login", "Account", null);
                    return;
                }

                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync($"{_apiBaseUrl}/Users/Permissions/{userId}");
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning($"Failed to get permissions for user {userId}. Status code: {response.StatusCode}");
                        context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                        return;
                    }

                    var permissions = await response.Content.ReadFromJsonAsync<UserPermissions>();
                    if (permissions == null)
                    {
                        _logger.LogWarning($"No permissions found for user {userId}");
                        context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                        return;
                    }

                    bool hasPermission = false;
                    switch (_permission)
                    {
                        case "CanViewSentences": hasPermission = permissions.CanViewSentences; break;
                        case "CanAddSentences": hasPermission = permissions.CanAddSentences; break;
                        case "CanEditSentences": hasPermission = permissions.CanEditSentences; break;
                        case "CanDeleteSentences": hasPermission = permissions.CanDeleteSentences; break;
                        case "CanViewWords": hasPermission = permissions.CanViewWords; break;
                        case "CanAddWords": hasPermission = permissions.CanAddWords; break;
                        case "CanEditWords": hasPermission = permissions.CanEditWords; break;
                        case "CanDeleteWords": hasPermission = permissions.CanDeleteWords; break;
                        case "CanViewMathOperations": hasPermission = permissions.CanViewMathOperations; break;
                        case "CanAddMathOperations": hasPermission = permissions.CanAddMathOperations; break;
                        case "CanEditMathOperations": hasPermission = permissions.CanEditMathOperations; break;
                        case "CanDeleteMathOperations": hasPermission = permissions.CanDeleteMathOperations; break;
                    }

                    if (!hasPermission)
                    {
                        _logger.LogWarning($"User {userId} does not have permission: {_permission}");
                        context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking permissions for permission: {_permission}");
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }
}
