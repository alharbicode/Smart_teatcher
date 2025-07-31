using System.ComponentModel.DataAnnotations;

namespace Smart_teatcher_MVC.Models
{
    public class MathExample
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "العملية الحسابية")]
        public ArithmeticOperations ArithmeticOperations { get; set; }

        [Display(Name = "الرقم الأول")]
        public int Num1 { get; set; }

        [Display(Name = "الرقم الثاني")]
        public int Num2 { get; set; }

        [Display(Name = "خطوات الحل")]
        public List<string> Steps { get; set; }

        [Display(Name = "المستوى")]
        public Level Level { get; set; }

        [Display(Name = "تاريخ الإنشاء")]
        [Required]
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
