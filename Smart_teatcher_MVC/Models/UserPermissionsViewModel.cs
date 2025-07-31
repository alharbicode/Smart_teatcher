using System.ComponentModel.DataAnnotations;

namespace Smart_teatcher_MVC.Models
{
    public class UserPermissionsViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }

        // Word Permissions
        [Display(Name = "إضافة كلمات جديدة")]
        public bool CanAddWords { get; set; }

        [Display(Name = "تعديل الكلمات")]
        public bool CanEditWords { get; set; }

        [Display(Name = "حذف الكلمات")]
        public bool CanDeleteWords { get; set; }

        [Display(Name = "عرض الكلمات")]
        public bool CanViewWords { get; set; }

        // Sentence Permissions
        [Display(Name = "إضافة جمل جديدة")]
        public bool CanAddSentences { get; set; }

        [Display(Name = "تعديل الجمل")]
        public bool CanEditSentences { get; set; }

        [Display(Name = "حذف الجمل")]
        public bool CanDeleteSentences { get; set; }

        [Display(Name = "عرض الجمل")]
        public bool CanViewSentences { get; set; }

        // Math Operations Permissions
        [Display(Name = "إضافة عمليات حسابية")]
        public bool CanAddMathOperations { get; set; }

        [Display(Name = "تعديل العمليات الحسابية")]
        public bool CanEditMathOperations { get; set; }

        [Display(Name = "حذف العمليات الحسابية")]
        public bool CanDeleteMathOperations { get; set; }

        [Display(Name = "عرض العمليات الحسابية")]
        public bool CanViewMathOperations { get; set; }
    }
}
