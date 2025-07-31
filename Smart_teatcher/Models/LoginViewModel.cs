using System.ComponentModel.DataAnnotations;

namespace Smart_teatcher.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        public string Username { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        public string Password { get; set; }

        // البريد الإلكتروني اختياري عند تسجيل الدخول
        public string? Email { get; set; }
    }
}
