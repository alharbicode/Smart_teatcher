using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_teatcher.Data;
using Smart_teatcher.Models;

namespace Smart_teatcher.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SentencesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SentencesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Sentences
      
        [HttpGet]
        public async Task<IActionResult> GetSentences()
        {
            var sentences = await _context.Sentences.ToListAsync();

            // حساب توقيع فريد بناءً على أحدث تعديل في البيانات
            var lastModified = _context.Sentences
                .OrderByDescending(s => EF.Property<DateTime>(s, "UpdatedAt"))
                .Select(s => EF.Property<DateTime>(s, "UpdatedAt"))
                .FirstOrDefault();

            var eTag = $"\"{lastModified.Ticks}\"";

            // التحقق من If-None-Match في الطلب
            if (Request.Headers.ContainsKey("If-None-Match") && Request.Headers["If-None-Match"] == eTag)
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            Response.Headers["ETag"] = eTag;
            return Ok(sentences);
        }

        // GET: api/Sentences/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSentence(int id)
        {
            var sentence = await _context.Sentences.FindAsync(id);

            if (sentence == null)
            {
                return NotFound();
            }

            // إنشاء eTag بناءً على توقيع البيانات
            var eTag = GenerateETag(sentence);

            // التحقق من If-None-Match في الطلب
            if (Request.Headers.ContainsKey("If-None-Match") && Request.Headers["If-None-Match"] == eTag)
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            Response.Headers["ETag"] = eTag;
            return Ok(sentence);
        }

        // POST: api/Sentences
        [HttpPost]
        public async Task<ActionResult<Sentence>> PostSentence(Sentence sentence)
        {
            sentence.UpdatedAt = DateTime.UtcNow;
            _context.Sentences.Add(sentence);
            await _context.SaveChangesAsync();

            var eTag = $"\"{sentence.UpdatedAt.Ticks}\"";
            Response.Headers["ETag"] = eTag;

            return CreatedAtAction("GetSentence", new { id = sentence.Id }, sentence);
        }

        // PUT: api/Sentences/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSentence(int id, Sentence sentence)
        {
            if (id != sentence.Id)
            {
                return BadRequest();
            }

            sentence.UpdatedAt = DateTime.UtcNow; // تحديث الطابع الزمني
            _context.Entry(sentence).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SentenceExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }





        // DELETE: api/Sentences/5
        // DELETE: api/Sentences/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSentence(int id)
        {
            var sentence = await _context.Sentences.FindAsync(id);
            if (sentence == null)
            {
                return NotFound();
            }

            // التحقق من If-Match في الطلب
            if (Request.Headers.ContainsKey("If-Match"))
            {
                var requestETag = Request.Headers["If-Match"]; // تغيير اسم المتغير إلى requestETag
                if (GenerateETag(sentence) != requestETag)
                {
                    return StatusCode(StatusCodes.Status412PreconditionFailed);
                }
            }

            // حذف الجملة من قاعدة البيانات
            _context.Sentences.Remove(sentence);
            await _context.SaveChangesAsync();

            // بعد الحذف، توليد ETag جديد بناءً على الوقت أو التغييرات في البيانات
            var lastModified = _context.Sentences
                .OrderByDescending(s => EF.Property<DateTime>(s, "UpdatedAt"))
                .Select(s => EF.Property<DateTime>(s, "UpdatedAt"))
                .FirstOrDefault();

            var updatedETag = $"\"{lastModified.Ticks}\""; // تغيير اسم المتغير إلى updatedETag

            // إضافة الـ ETag الجديد في الـ response headers
            Response.Headers["ETag"] = updatedETag;

            return NoContent();
        }



        private bool SentenceExists(int id)
        {
            return _context.Sentences.Any(e => e.Id == id);
        }

        private string GenerateETag(Sentence sentence)
        {
            // إنشاء eTag باستخدام خاصية Timestamp أو البيانات الأخرى الفريدة
            return $"\"{sentence.GetHashCode()}\"";
        }
    }
}
