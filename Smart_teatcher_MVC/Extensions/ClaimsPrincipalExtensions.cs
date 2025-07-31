using System.Security.Claims;
using System.Net.Http.Json;
using Smart_teatcher_MVC.Models;

namespace Smart_teatcher_MVC.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        private static readonly string _apiBaseUrl = $"http://{connected_ip.connection}/api";

        public static bool HasPermission(this ClaimsPrincipal user, string permission)
        {
            if (user == null) return false;
            if (user.IsInRole("Root")) return true;

            // Get userId from claims
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return false;

            // Get permissions from API
            using (var client = new HttpClient())
            {
                try
                {
                    var response = client.GetAsync($"{_apiBaseUrl}/Users/Permissions/{userId}").Result;
                    if (!response.IsSuccessStatusCode)
                        return false;

                    var permissions = response.Content.ReadFromJsonAsync<UserPermissions>().Result;
                    if (permissions == null)
                        return false;

                    return permission switch
                    {
                        "CanViewSentences" => permissions.CanViewSentences,
                        "CanAddSentences" => permissions.CanAddSentences,
                        "CanEditSentences" => permissions.CanEditSentences,
                        "CanDeleteSentences" => permissions.CanDeleteSentences,
                        "CanViewWords" => permissions.CanViewWords,
                        "CanAddWords" => permissions.CanAddWords,
                        "CanEditWords" => permissions.CanEditWords,
                        "CanDeleteWords" => permissions.CanDeleteWords,
                        "CanViewMathOperations" => permissions.CanViewMathOperations,
                        "CanAddMathOperations" => permissions.CanAddMathOperations,
                        "CanEditMathOperations" => permissions.CanEditMathOperations,
                        "CanDeleteMathOperations" => permissions.CanDeleteMathOperations,
                        _ => false
                    };
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
