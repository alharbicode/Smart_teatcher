using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Smart_teatcher_MVC.Models;
using System.Text;

namespace Smart_teatcher_MVC.Controllers
{
    public class WordsController : Controller
    {
        private readonly string apiUrl = $"http://{connected_ip.connection}/api/Words";

        // GET: Words
        public async Task<IActionResult> Index()
        {
            List<Word> words = new List<Word>();
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(apiUrl))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        words = JsonConvert.DeserializeObject<List<Word>>(content);
                    }
                }
            }
            return View(words);
        }


        // عرض تفاصيل جملة معينة
        public async Task<IActionResult> Details(int id)
        {
            Word word = null;
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync($"{apiUrl}/{id}"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        word = JsonConvert.DeserializeObject<Word>(content);
                    }
                }
            }
            if (word == null)
            {
                return NotFound();
            }
            return View(word);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Word word, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(word);
            }

            if (imageFile != null)
            {
                word.Image = ConvertImageToByteArray(imageFile);
            }

            using (var httpClient = new HttpClient())
            {
                StringContent stringContent = new StringContent(JsonConvert.SerializeObject(word), Encoding.UTF8, "application/json");
                using (var response = await httpClient.PostAsync(apiUrl, stringContent))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            return View(word);
        }

        public async Task<IActionResult> Edit(int id)
        {
            Word word = null;
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync($"{apiUrl}/{id}"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        word = JsonConvert.DeserializeObject<Word>(content);
                    }
                }
            }
            if (word == null)
            {
                return NotFound();
            }
            return View(word);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Word word, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(word);
            }

            if (imageFile != null)
            {
                word.Image = ConvertImageToByteArray(imageFile);
            }

            using (var httpClient = new HttpClient())
            {
                StringContent stringContent = new StringContent(JsonConvert.SerializeObject(word), Encoding.UTF8, "application/json");
                using (var response = await httpClient.PutAsync($"{apiUrl}/{word.Id}", stringContent))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            return View(word);
        }
        public async Task<IActionResult> Delete(int id)
        {
            Word word = new Word();
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync($"{apiUrl}/{id}"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        word = JsonConvert.DeserializeObject<Word>(content);
                    }
                }
            }

            return View(word);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        private byte[] ConvertImageToByteArray(IFormFile imageFile)
        {
            using (var memoryStream = new MemoryStream())
            {
                imageFile.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
