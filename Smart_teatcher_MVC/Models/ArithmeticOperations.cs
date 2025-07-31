using System.ComponentModel.DataAnnotations;

namespace Smart_teatcher_MVC.Models
{
    public enum ArithmeticOperations
    {
        [Display(Name = "جمع")]
        Addition,
        [Display(Name = "طرح")]
        Subtraction,
        [Display(Name = "ضرب")]
        Multiplication,
        [Display(Name = "قسمة")]
        Division
    }

}
