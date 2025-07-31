using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_teatcher.Data;
using Smart_teatcher.Models;
using System.Security.Cryptography;
using System.Text;

namespace Smart_teatcher.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // GET: api/Users/Count
        [HttpGet("Count")]
        public async Task<ActionResult<int>> GetUsersCount()
        {
            return await _context.Users.CountAsync();
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            var userResponses = users.Select(user => new User
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive
            }).ToList();

            return userResponses;
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var userResponse = new User
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive
            };

            return userResponse;
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, LoginViewModel model)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // تحديث البيانات
            user.Username = model.Username;
            user.Email = model.Email;
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.PasswordHash = HashPassword(model.Password);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        // PUT: api/Users/Update/5
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] LoginViewModel model)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("المستخدم غير موجود");
            }

            // لا يمكن تعديل حساب root
            if (user.Role == "Root")
            {
                return BadRequest("لا يمكن تعديل حساب المدير الرئيسي");
            }

            // التحقق من عدم تكرار اسم المستخدم أو البريد الإلكتروني
            var exists = await _context.Users
                .AnyAsync(u => (u.Username == model.Username || u.Email == model.Email) && u.Id != id);
            if (exists)
            {
                return BadRequest("اسم المستخدم أو البريد الإلكتروني مستخدم بالفعل");
            }

            user.Username = model.Username;
            user.Email = model.Email;
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.PasswordHash = HashPassword(model.Password);
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("المستخدم غير موجود");
            }

            // لا يمكن حذف حساب root
            if (user.Role == "Root")
            {
                return BadRequest("لا يمكن حذف حساب المدير الرئيسي");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost("Login")]
        public async Task<ActionResult<User>> Login([FromBody] LoginViewModel model)
        {
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("اسم المستخدم وكلمة المرور مطلوبة");
            }

            var hashedPassword = HashPassword(model.Password);
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user == null)
            {
                return Unauthorized("اسم المستخدم أو كلمة المرور غير صحيحة");
            }

            if (user.PasswordHash != hashedPassword)
            {
                return Unauthorized("اسم المستخدم أو كلمة المرور غير صحيحة");
            }

            if (!user.IsActive)
            {
                return Unauthorized("الحساب غير نشط");
            }

            user.LastLoginAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // إنشاء نسخة جديدة من المستخدم للرد
            var userResponse = new User
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive
            };

            return userResponse;
        }

        [HttpGet("HasRoot")]
        public async Task<ActionResult<bool>> HasRoot()
        {
            var rootExists = await _context.Users.AnyAsync(u => u.Role == "Root");
            return rootExists;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<User>> Register([FromBody] LoginViewModel model)
        {
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.Email))
            {
                return BadRequest("جميع الحقول مطلوبة للتسجيل");
            }

            var userExists = await _context.Users.AnyAsync(u => u.Username == model.Username);
            if (userExists)
            {
                return BadRequest("اسم المستخدم مستخدم بالفعل");
            }

            // التحقق مما إذا كان هناك مستخدم root
            var hasRoot = await _context.Users.AnyAsync(u => u.Role == "Root");

            var hashedPassword = HashPassword(model.Password);
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = hashedPassword,
                Role = hasRoot ? "Admin" : "Root",
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // إنشاء نسخة جديدة من المستخدم للرد
            var userResponse = new User
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive
            };

            return userResponse;
        }

        [HttpPost("ToggleActive/{id}")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("المستخدم غير موجود");
            }

            // لا يمكن تعطيل حساب root
            if (user.Role == "Root")
            {
                return BadRequest("لا يمكن تعطيل حساب المدير الرئيسي");
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, isActive = user.IsActive });
        }

        // GET: api/Users/Permissions/5
        [HttpGet("Permissions/{id}")]
        public async Task<ActionResult<UserPermissions>> GetUserPermissions(int id)
        {
            var user = await _context.Users
                .Include(u => u.Permissions)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new { success = false, message = "المستخدم غير موجود" });
            }

            if (user.Permissions == null)
            {
                // إذا لم تكن هناك صلاحيات، نقوم بإنشاء صلاحيات افتراضية
                var defaultPermissions = new UserPermissions
                {
                    UserId = id,
                    CanViewSentences = true,
                    CanAddSentences = false,
                    CanEditSentences = false,
                    CanDeleteSentences = false,
                    CanViewWords = true,
                    CanAddWords = false,
                    CanEditWords = false,
                    CanDeleteWords = false,
                    CanViewMathOperations = true,
                    CanAddMathOperations = false,
                    CanEditMathOperations = false,
                    CanDeleteMathOperations = false
                };

                _context.UserPermissions.Add(defaultPermissions);
                await _context.SaveChangesAsync();
                return defaultPermissions;
            }

            return user.Permissions;
        }

        // PUT: api/Users/Permissions/5
        [HttpPut("Permissions/{id}")]
        public async Task<IActionResult> UpdatePermissions(int id, [FromBody] UpdatePermissionsDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Permissions)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new { success = false, message = "المستخدم غير موجود" });
            }

            if (user.Role == "Root")
            {
                return BadRequest(new { success = false, message = "لا يمكن تعديل صلاحيات المدير الرئيسي" });
            }

            try
            {
                if (user.Permissions == null)
                {
                    // إنشاء صلاحيات جديدة
                    var permissions = new UserPermissions
                    {
                        UserId = id,
                        CanViewSentences = dto.CanViewSentences,
                        CanAddSentences = dto.CanAddSentences,
                        CanEditSentences = dto.CanEditSentences,
                        CanDeleteSentences = dto.CanDeleteSentences,
                        CanViewWords = dto.CanViewWords,
                        CanAddWords = dto.CanAddWords,
                        CanEditWords = dto.CanEditWords,
                        CanDeleteWords = dto.CanDeleteWords,
                        CanViewMathOperations = dto.CanViewMathOperations,
                        CanAddMathOperations = dto.CanAddMathOperations,
                        CanEditMathOperations = dto.CanEditMathOperations,
                        CanDeleteMathOperations = dto.CanDeleteMathOperations
                    };
                    _context.UserPermissions.Add(permissions);
                }
                else
                {
                    // تحديث الصلاحيات الموجودة
                    user.Permissions.CanViewSentences = dto.CanViewSentences;
                    user.Permissions.CanAddSentences = dto.CanAddSentences;
                    user.Permissions.CanEditSentences = dto.CanEditSentences;
                    user.Permissions.CanDeleteSentences = dto.CanDeleteSentences;
                    user.Permissions.CanViewWords = dto.CanViewWords;
                    user.Permissions.CanAddWords = dto.CanAddWords;
                    user.Permissions.CanEditWords = dto.CanEditWords;
                    user.Permissions.CanDeleteWords = dto.CanDeleteWords;
                    user.Permissions.CanViewMathOperations = dto.CanViewMathOperations;
                    user.Permissions.CanAddMathOperations = dto.CanAddMathOperations;
                    user.Permissions.CanEditMathOperations = dto.CanEditMathOperations;
                    user.Permissions.CanDeleteMathOperations = dto.CanDeleteMathOperations;
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "تم تحديث الصلاحيات بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "حدث خطأ أثناء حفظ الصلاحيات", error = ex.Message });
            }
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
