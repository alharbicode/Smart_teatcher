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
    public class WordsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Words
        [HttpGet]
        public async Task<IActionResult> GetWords()
        {
            var words = await _context.Words.ToListAsync();

            // حساب أحدث تعديل في البيانات
            var lastModified = _context.Words
                .OrderByDescending(w => EF.Property<DateTime>(w, "UpdatedAt"))
                .Select(w => EF.Property<DateTime>(w, "UpdatedAt"))
                .FirstOrDefault();

            var eTag = $"\"{lastModified.Ticks}\"";

            // التحقق من If-None-Match في الطلب
            if (Request.Headers.ContainsKey("If-None-Match") && Request.Headers["If-None-Match"] == eTag)
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            Response.Headers["ETag"] = eTag;
            return Ok(words);
        }

        // GET: api/Words/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWord(int id)
        {
            var word = await _context.Words.FindAsync(id);

            if (word == null)
            {
                return NotFound();
            }

            // إنشاء eTag بناءً على توقيع البيانات
            var eTag = GenerateETag(word);

            // التحقق من If-None-Match في الطلب
            if (Request.Headers.ContainsKey("If-None-Match") && Request.Headers["If-None-Match"] == eTag)
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            Response.Headers["ETag"] = eTag;
            return Ok(word);
        }

        // POST: api/Words
        [HttpPost]
        public async Task<ActionResult<Word>> PostWord(Word word)
        {
            word.UpdatedAt = DateTime.UtcNow; // تعيين تاريخ التحديث
            _context.Words.Add(word);
            await _context.SaveChangesAsync();

            var eTag = $"\"{word.UpdatedAt.Ticks}\"";
            Response.Headers["ETag"] = eTag;

            return CreatedAtAction(nameof(GetWord), new { id = word.Id }, word);
        }

        // PUT: api/Words/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWord(int id, Word word)
        {
            if (id != word.Id)
            {
                return BadRequest();
            }

            word.UpdatedAt = DateTime.UtcNow; // تحديث الطابع الزمني
            _context.Entry(word).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WordExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // لا حاجة لتحديث ETag هنا، سيتم تحديثه في GetWords

            return NoContent();
        }

        // DELETE: api/Words/5
        // DELETE: api/Words/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWord(int id)
        {
            var word = await _context.Words.FindAsync(id);
            if (word == null)
            {
                return NotFound();
            }

            // حساب ETag قبل الحذف استنادًا إلى "UpdatedAt"
            var eTag = $"\"{word.UpdatedAt.Ticks}-{word.Id}\"";  // دمج ID مع Ticks

            // التحقق من If-Match في الطلب
            if (Request.Headers.ContainsKey("If-Match"))
            {
                if (Request.Headers["If-Match"] == eTag)
                {
                    _context.Words.Remove(word);
                    await _context.SaveChangesAsync();

                    // بعد الحذف، يمكن إعادة توليد ETag جديد بناءً على الحذف
                    Response.Headers["ETag"] = eTag; // الـ ETag يبقى نفسه لأن الحذف تم بنجاح
                    return NoContent();
                }
                else
                {
                    return StatusCode(StatusCodes.Status412PreconditionFailed);
                }
            }
            else
            {
                // If-Match غير موجود، احذف الكلمة مباشرة
                _context.Words.Remove(word);
                await _context.SaveChangesAsync();

                // إعادة توليد ETag بعد الحذف مباشرة (نفس الـ ETag المستخدم قبل الحذف)
                Response.Headers["ETag"] = eTag;
                return NoContent();
            }
        }

        private bool WordExists(int id)
        {
            return _context.Words.Any(e => e.Id == id);
        }

        private string GenerateETag(Word word)
        {
            // استخدام HashCode الخاص بالكائن ك ETag
            return $"\"{word.GetHashCode()}\"";
        }
    }
}