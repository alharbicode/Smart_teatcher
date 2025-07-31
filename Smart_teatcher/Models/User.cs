using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_teatcher.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string PasswordHash { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLoginAt { get; set; }

        // Foreign key for the user who created this user
        public int? CreatedBy { get; set; }

        // Navigation property to the user who created this user
        [ForeignKey("CreatedBy")]
        public virtual User Creator { get; set; }

        // العلاقة مع الصلاحيات
        public virtual UserPermissions Permissions { get; set; }
    }
}
