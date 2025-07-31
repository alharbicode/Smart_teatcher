using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Smart_teatcher.Models
{
    public class UserPermissions
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [JsonIgnore]
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        // صلاحيات الجمل
        public bool CanAddSentences { get; set; }
        public bool CanEditSentences { get; set; }
        public bool CanDeleteSentences { get; set; }
        public bool CanViewSentences { get; set; }

        // صلاحيات الكلمات
        public bool CanAddWords { get; set; }
        public bool CanEditWords { get; set; }
        public bool CanDeleteWords { get; set; }
        public bool CanViewWords { get; set; }

        // صلاحيات العمليات الحسابية
        public bool CanAddMathOperations { get; set; }
        public bool CanEditMathOperations { get; set; }
        public bool CanDeleteMathOperations { get; set; }
        public bool CanViewMathOperations { get; set; }
    }
}
