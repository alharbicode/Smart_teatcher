using System.ComponentModel.DataAnnotations;

namespace Smart_teatcher.Models
{
    public class MathExample
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "الرجاء اختيار نوع العملية الحسابية")]
        [Display(Name = "نوع العملية")]
        public ArithmeticOperations ArithmeticOperations { get; set; } // نوع العملية الحسابية : جمع ، طرح ، ضرب ، قسمة

        [Required(ErrorMessage = "الرجاء إدخال الرقم الأول")]
        [Display(Name = "الرقم الأول")]
        public int Num1 { get; set; }

        [Required(ErrorMessage = "الرجاء إدخال الرقم الثاني")]
        [Display(Name = "الرقم الثاني")]
        public int Num2 { get; set; }

        [Required]
        public List<string> Steps { get; set; } = new(); // خطوات الحل

        [Required(ErrorMessage = "الرجاء اختيار المستوى")]
        [Display(Name = "المستوى")]
        public Level Level { get; set; } // مستوى الجملة  : مبتدئ، متوسط، متقدم


        /*
         * الـ etag
         * الزمني لآخر تعديل
         */
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
