using System.ComponentModel.DataAnnotations;

namespace Smart_teatcher_MVC.Models
{
    public class Word
    {
        [Key]
        public int Id { get; set; } // معرف الكلمة

        [Required(ErrorMessage = "الرجاء إدخال نص الكلمة")]
        [Display(Name = "نص الكلمة")]
        public string Text { get; set; } // نص الكلمة

        [Display(Name = "صورة الكلمة")]
        public byte[]? Image { get; set; } // صورة الكلمة كـ byte array

        [Required(ErrorMessage = "الرجاء اختيار مستوى الكلمة")]
        [Display(Name = "المستوى")]
        public Level Level { get; set; } // مستوى الكلمة (مبتدئ، متوسط، متقدم)

        [Required]
        [Display(Name = "تاريخ الإنشاء")]
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; } = DateTime.Now; // تاريخ الإنشاء
    }
}
