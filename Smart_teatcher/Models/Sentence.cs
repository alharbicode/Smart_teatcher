using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Smart_teatcher.Models
{
    /// <summary>
    /// نموذج الجملة العربية
    /// </summary>
    public class Sentence
    {
        public int Id { get; set; } // معرف الجملة
        public string Text { get; set; } // نص الجملة
        public Level Level { get; set; } // مستوى الجملة (مبتدئ، متوسط، متقدم)

        // الطابع الزمني لآخر تعديل
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
