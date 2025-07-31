using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Smart_teatcher_MVC.Models;
using Smart_teatcher_MVC.Filters;
using System.Text;

namespace Smart_teatcher_MVC.Controllers
{
    public class SentencesController : Controller
    {
        private readonly string apiUrl = $"http://{connected_ip.connection}/api/Sentences";
        private readonly ILogger<SentencesController> _logger;

        public SentencesController(ILogger<SentencesController> logger)
        {
            _logger = logger;
        }

        // عرض قائمة الجمل
        public async Task<IActionResult> Index()
        {
            try
            {
                List<Sentence> sentences = new List<Sentence>();
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(apiUrl))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            sentences = JsonConvert.DeserializeObject<List<Sentence>>(content);
                        }
                    }
                }
                return View(sentences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sentences");
                TempData["Error"] = "حدث خطأ أثناء جلب قائمة الجمل";
                return View(new List<Sentence>());
            }
        }

        // عرض تفاصيل جملة معينة
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                Sentence sentence = null;
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync($"{apiUrl}/{id}"))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            sentence = JsonConvert.DeserializeObject<Sentence>(content);
                        }
                    }
                }
                return View(sentence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving sentence details for ID: {id}");
                TempData["Error"] = "حدث خطأ أثناء جلب تفاصيل الجملة";
                return RedirectToAction(nameof(Index));
            }
        }

        // عرض نموذج الإضافة
        [RequirePermission("CanAddSentences")]
        public IActionResult Create()
        {
            return View();
        }

        // حفظ الجملة الجديدة
        [HttpPost]
        [RequirePermission("CanAddSentences")]
        public async Task<IActionResult> Create(Sentence sentence)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (var httpClient = new HttpClient())
                    {
                        var json = JsonConvert.SerializeObject(sentence);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        using (var response = await httpClient.PostAsync(apiUrl, content))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                TempData["Success"] = "تمت إضافة الجملة بنجاح";
                                return RedirectToAction(nameof(Index));
                            }
                        }
                    }
                }
                return View(sentence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new sentence");
                TempData["Error"] = "حدث خطأ أثناء إضافة الجملة";
                return View(sentence);
            }
        }

        // عرض نموذج التعديل
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                Sentence sentence = null;
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync($"{apiUrl}/{id}"))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            sentence = JsonConvert.DeserializeObject<Sentence>(content);
                        }
                    }
                }
                return View(sentence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving sentence for edit, ID: {id}");
                TempData["Error"] = "حدث خطأ أثناء جلب بيانات الجملة للتعديل";
                return RedirectToAction(nameof(Index));
            }
        }

        // حفظ التعديلات
        [HttpPost]
        [RequirePermission("CanEditSentences")]
        public async Task<IActionResult> Edit(int id, Sentence sentence)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (var httpClient = new HttpClient())
                    {
                        var json = JsonConvert.SerializeObject(sentence);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        using (var response = await httpClient.PutAsync($"{apiUrl}/{id}", content))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                TempData["Success"] = "تم تحديث الجملة بنجاح";
                                return RedirectToAction(nameof(Index));
                            }
                        }
                    }
                }
                return View(sentence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating sentence, ID: {id}");
                TempData["Error"] = "حدث خطأ أثناء تحديث الجملة";
                return View(sentence);
            }
        }

        // عرض صفحة تأكيد الحذف
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                Sentence sentence = null;
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync($"{apiUrl}/{id}"))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            sentence = JsonConvert.DeserializeObject<Sentence>(content);
                        }
                    }
                }
                if (sentence == null)
                {
                    return NotFound();
                }
                return View(sentence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving sentence for deletion, ID: {id}");
                TempData["Error"] = "حدث خطأ أثناء جلب بيانات الجملة للحذف";
                return RedirectToAction(nameof(Index));
            }
        }

        // تنفيذ عملية الحذف
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("CanDeleteSentences")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.DeleteAsync($"{apiUrl}/{id}"))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            TempData["Success"] = "تم حذف الجملة بنجاح";
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
                TempData["Error"] = "فشل حذف الجملة";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting sentence, ID: {id}");
                TempData["Error"] = "حدث خطأ أثناء محاولة حذف الجملة";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}