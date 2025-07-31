using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Smart_teatcher_MVC.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Smart_teatcher_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string apiBaseUrl = $"http://{connected_ip.connection}/api"; // تحديث عنوان الـ API

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel();

            using (var httpClient = new HttpClient())
            {
                try
                {
                    // جلب العمليات الحسابية
                    var mathResponse = await httpClient.GetAsync($"{apiBaseUrl}/MathExamples");
                    if (mathResponse.IsSuccessStatusCode)
                    {
                        var mathContent = await mathResponse.Content.ReadAsStringAsync();
                        var mathExamples = JsonConvert.DeserializeObject<List<MathExample>>(mathContent);
                        if (mathExamples != null)
                        {
                            viewModel.TotalMathExamples = mathExamples.Count;
                            viewModel.RecentMathExamples = mathExamples.OrderByDescending(m => m.CreatedAt).Take(5).ToList();
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"فشل في جلب العمليات الحسابية. الحالة: {mathResponse.StatusCode}");
                    }

                    // جلب الكلمات
                    var wordsResponse = await httpClient.GetAsync($"{apiBaseUrl}/Words");
                    if (wordsResponse.IsSuccessStatusCode)
                    {
                        var wordsContent = await wordsResponse.Content.ReadAsStringAsync();
                        var words = JsonConvert.DeserializeObject<List<Word>>(wordsContent);
                        if (words != null)
                        {
                            viewModel.TotalWords = words.Count;
                            viewModel.RecentWords = words.OrderByDescending(w => w.CreatedAt).Take(5).ToList();
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"فشل في جلب الكلمات. الحالة: {wordsResponse.StatusCode}");
                    }

                    // جلب الجمل
                    var sentencesResponse = await httpClient.GetAsync($"{apiBaseUrl}/Sentences");
                    if (sentencesResponse.IsSuccessStatusCode)
                    {
                        var sentencesContent = await sentencesResponse.Content.ReadAsStringAsync();
                        var sentences = JsonConvert.DeserializeObject<List<Sentence>>(sentencesContent);
                        if (sentences != null)
                        {
                            viewModel.TotalSentences = sentences.Count;
                            viewModel.RecentSentences = sentences.OrderByDescending(s => s.CreatedAt).Take(5).ToList();
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"فشل في جلب الجمل. الحالة: {sentencesResponse.StatusCode}");
                    }

                    // جلب عدد المستخدمين
                    var usersResponse = await httpClient.GetAsync($"{apiBaseUrl}/Users/Count");
                    if (usersResponse.IsSuccessStatusCode)
                    {
                        var usersCount = await usersResponse.Content.ReadAsStringAsync();
                        viewModel.TotalUsers = int.Parse(usersCount);
                    }
                    else
                    {
                        _logger.LogWarning($"فشل في جلب عدد المستخدمين. الحالة: {usersResponse.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "حدث خطأ أثناء جلب بيانات لوحة التحكم");
                    // إضافة رسالة خطأ للمستخدم
                    TempData["Error"] = "حدث خطأ أثناء جلب البيانات. يرجى المحاولة مرة أخرى لاحقاً.";
                }
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
