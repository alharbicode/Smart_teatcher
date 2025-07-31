using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Smart_teatcher.Models
{
    /// <summary>
    /// نموذج الكلمة العربية
    /// </summary>
    public class Word
    {
        public int Id { get; set; } // معرف الكلمة
        public string Text { get; set; } // نص الكلمة
        public byte[]? Image { get; set; } // صورة الكلمة كـ byte array
        public Level Level { get; set; } // مستوى الكلمة (مبتدئ، متوسط، متقدم)

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }


}
