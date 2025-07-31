using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Smart_teatcher_MVC.Models;
using Smart_teatcher_MVC.Filters;
using System.Text;

namespace Smart_teatcher_MVC.Controllers
{
    public class MathExamplesController : Controller
    {
        private readonly string apiUrl = $"http://{connected_ip.connection}/api/MathExamples";

        // عرض قائمة العمليات
        public async Task<IActionResult> Index()
        {
            List<MathExample> mathExamples = new List<MathExample>();
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(apiUrl))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        mathExamples = JsonConvert.DeserializeObject<List<MathExample>>(content);
                    }
                }
            }
            return View(mathExamples);
        }

        // عرض تفاصيل جملة معينة
        public async Task<IActionResult> Details(int id)
        {
            MathExample mathExample = null;
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync($"{apiUrl}/{id}"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        mathExample = JsonConvert.DeserializeObject<MathExample>(content);
                    }
                }
            }
            return View(mathExample);
        }

        // عرض نموذج الإضافة
        [RequirePermission("CanAddMathOperations")]
        public IActionResult Create()
        {
            return View();
        }

        // تنفيذ عملية الإضافة
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("CanAddMathOperations")]
        public async Task<IActionResult> Create(MathExample mathExample)
        {
            if (ModelState.IsValid)
            {
                using (var httpClient = new HttpClient())
                {
                    StringContent stringContent = new StringContent(JsonConvert.SerializeObject(mathExample), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync(apiUrl, stringContent))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
            }
            return View(mathExample);
        }

        // عرض نموذج التعديل
        [RequirePermission("CanEditMathOperations")]
        public async Task<IActionResult> Edit(int id)
        {
            MathExample mathExample = null;
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync($"{apiUrl}/{id}"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        mathExample = JsonConvert.DeserializeObject<MathExample>(content);
                    }
                }
            }
            return View(mathExample);
        }

        // تنفيذ عملية التعديل
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("CanEditMathOperations")]
        public async Task<IActionResult> Edit(MathExample mathExample)
        {
            if (ModelState.IsValid)
            {
                using (var httpClient = new HttpClient())
                {
                    StringContent stringContent = new StringContent(JsonConvert.SerializeObject(mathExample), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PutAsync($"{apiUrl}/{mathExample.Id}", stringContent))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
            }
            return View(mathExample);
        }

        // عرض صفحة الحذف
        [RequirePermission("CanDeleteMathOperations")]
        public async Task<IActionResult> Delete(int id)
        {
            MathExample mathExample = null;
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync($"{apiUrl}/{id}"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        mathExample = JsonConvert.DeserializeObject<MathExample>(content);
                    }
                }
            }
            return View(mathExample);
        }

        // تنفيذ عملية الحذف
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("CanDeleteMathOperations")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.DeleteAsync($"{apiUrl}/{id}"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            return View();
        }
    }
}
