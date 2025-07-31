using System.ComponentModel.DataAnnotations;

namespace Smart_teatcher_MVC.Models
{
    public class Sentence
    {
        [Key]
        public int Id { get; set; } // معرف الجملة

        [Required(ErrorMessage = "الرجاء إدخال نص الجملة")]
        [Display(Name = "نص الجملة")]
        public string Text { get; set; } // نص الجملة

        [Required(ErrorMessage = "الرجاء اختيار مستوى الجملة")]
        [Display(Name = "المستوى")]
        public Level Level { get; set; } // مستوى الجملة (مبتدئ، متوسط، متقدم)

        [Required]
        [Display(Name = "تاريخ الإنشاء")]
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;// تاريخ الإنشاء
    }
}
