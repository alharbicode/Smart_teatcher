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
    public class MathExamplesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MathExamplesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/MathExamples
        [HttpGet]
        public async Task<IActionResult> GetMathExample()
        {
            var mathExamples = await _context.MathExample.ToListAsync();

            // حساب توقيع فريد بناءً على أحدث تعديل في البيانات
            DateTime? lastModified = await _context.MathExample
                .OrderByDescending(s => EF.Property<DateTime>(s, "UpdatedAt"))
                .Select(s => EF.Property<DateTime>(s, "UpdatedAt"))
                .FirstOrDefaultAsync();

            var eTag = lastModified.HasValue ? $"\"{lastModified.Value.Ticks}\"" : "\"\""; // أو قيمة افتراضية أخرى

            // التحقق من If-None-Match في الطلب
            if (Request.Headers.ContainsKey("If-None-Match") && Request.Headers["If-None-Match"] == eTag)
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            Response.Headers["ETag"] = eTag;
            return Ok(mathExamples);
        }
        // GET: api/MathExamples/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMathExample(int id)
        {
            var mathExample = await _context.MathExample.FindAsync(id);

            if (mathExample == null)
            {
                return NotFound();
            }

            // إنشاء eTag بناءً على توقيع البيانات
            var eTag = GenerateETag(mathExample);

            // التحقق من If-None-Match في الطلب
            if (Request.Headers.ContainsKey("If-None-Match") && Request.Headers["If-None-Match"] == eTag)
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            Response.Headers["ETag"] = eTag;
            return Ok(mathExample);
        }

        // PUT: api/MathExamples/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMathExample(int id, MathExample mathExample)
        {
            if (id != mathExample.Id)
            {
                return BadRequest();
            }

            mathExample.UpdatedAt = DateTime.UtcNow; // تحديث الطابع الزمني
            _context.Entry(mathExample).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MathExampleExists(id))
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

        // POST: api/MathExamples
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MathExample>> PostMathExample(MathExample mathExample)
        {
            mathExample.UpdatedAt = DateTime.UtcNow;
            _context.MathExample.Add(mathExample);
            await _context.SaveChangesAsync();

            var eTag = $"\"{mathExample.UpdatedAt.Ticks}\"";
            Response.Headers["ETag"] = eTag;

            return CreatedAtAction("GetMathExample", new { id = mathExample.Id }, mathExample);
        }

        // DELETE: api/MathExamples/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMathExample(int id)
        {
            var mathExample = await _context.MathExample.FindAsync(id);
            if (mathExample == null)
            {
                return NotFound();
            }

            // التحقق من If-Match في الطلب
            if (Request.Headers.ContainsKey("If-Match"))
            {
                var requestETag = Request.Headers["If-Match"];
                if (GenerateETag(mathExample) != requestETag)
                {
                    return StatusCode(StatusCodes.Status412PreconditionFailed);
                }
            }

            _context.MathExample.Remove(mathExample);
            await _context.SaveChangesAsync();

            // بعد الحذف، توليد ETag جديد بناءً على الوقت أو التغييرات في البيانات
            var lastModified = _context.MathExample
                .OrderByDescending(s => EF.Property<DateTime>(s, "UpdatedAt"))
                .Select(s => EF.Property<DateTime>(s, "UpdatedAt"))
                .FirstOrDefault();

            var updatedETag = $"\"{lastModified.Ticks}\"";

            // إضافة الـ ETag الجديد في الـ response headers
            Response.Headers["ETag"] = updatedETag;

            return NoContent();
        }

        private bool MathExampleExists(int id)
        {
            return _context.MathExample.Any(e => e.Id == id);
        }

        private string GenerateETag(MathExample mathExample)
        {
            // إنشاء eTag باستخدام خاصية Timestamp أو البيانات الأخرى الفريدة
            return $"\"{mathExample.GetHashCode()}\"";
        }
    }
}