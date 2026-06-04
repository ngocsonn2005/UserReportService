using System;

namespace UserReportService.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }           // ✅ Email người dùng

        public string Phone { get; set; }           // ✅ Số điện thoại

        public string Role { get; set; }

        public string? Avatar { get; set; }          // ✅ URL avatar

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public bool IsLocked { get; set; } = false;

        public DateTime? LockedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }  // ✅ Lần đăng nhập cuối

        public int LoginCount { get; set; } = 0;    // ✅ Số lần đăng nhập
    }
}