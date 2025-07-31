using System.Collections.Generic;

namespace Smart_teatcher_MVC.Models
{
    public class DashboardViewModel
    {
        // إحصائيات العمليات الحسابية
        public int TotalMathExamples { get; set; }
        public List<MathExample> RecentMathExamples { get; set; }

        // إحصائيات الكلمات
        public int TotalWords { get; set; }
        public List<Word> RecentWords { get; set; }

        // إحصائيات الجمل
        public int TotalSentences { get; set; }
        public List<Sentence> RecentSentences { get; set; }

        // إحصائيات المستخدمين
        public int TotalUsers { get; set; }

        public DashboardViewModel()
        {
            RecentMathExamples = new List<MathExample>();
            RecentWords = new List<Word>();
            RecentSentences = new List<Sentence>();
        }
    }
}
