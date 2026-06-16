using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserReportService.Data;
using UserReportService.Models;
using UserReportService.DTOs;
using System;
using System.Linq;
using System.Security.Claims;

namespace UserReportService.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Lấy danh sách user chưa bị xóa (đang hoạt động)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAll()
        {
            var users = _context.Users
                .Where(u => !u.IsDeleted)
                .Select(u => new UserResponseDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    Avatar = u.Avatar,
                    Role = u.Role,
                    IsLocked = u.IsLocked,
                    RequirePasswordChange = u.RequirePasswordChange, // ✅ THÊM
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    LoginCount = u.LoginCount
                }).ToList();

            return Ok(users);
        }

        // ✅ Lấy danh sách user đã xóa (trong thùng rác)
        [HttpGet("deleted")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetDeleted()
        {
            var users = _context.Users
                .Where(u => u.IsDeleted)
                .Select(u => new UserResponseDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    Avatar = u.Avatar,
                    Role = u.Role,
                    IsLocked = u.IsLocked,
                    RequirePasswordChange = u.RequirePasswordChange, // ✅ THÊM
                    CreatedAt = u.CreatedAt,
                    DeletedAt = u.DeletedAt
                }).ToList();

            return Ok(users);
        }

        // ✅ Tìm kiếm user
        [HttpGet("search")]
        [Authorize(Roles = "Admin")]
        public IActionResult Search([FromQuery] string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return Ok(new List<UserResponseDTO>());

            var users = _context.Users
                .Where(u => !u.IsDeleted &&
                    (u.Username.Contains(keyword) ||
                     u.FullName.Contains(keyword) ||
                     u.Email.Contains(keyword) ||
                     u.Phone.Contains(keyword) ||
                     u.Role.Contains(keyword)))
                .Select(u => new UserResponseDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    Avatar = u.Avatar,
                    Role = u.Role,
                    IsLocked = u.IsLocked,
                    RequirePasswordChange = u.RequirePasswordChange, // ✅ THÊM
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    LoginCount = u.LoginCount
                }).ToList();

            return Ok(users);
        }

        // ✅ Lấy user theo Role
        [HttpGet("by-role/{role}")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetByRole(string role)
        {
            var users = _context.Users
                .Where(u => !u.IsDeleted && u.Role == role)
                .Select(u => new UserResponseDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    Avatar = u.Avatar,
                    Role = u.Role,
                    IsLocked = u.IsLocked,
                    RequirePasswordChange = u.RequirePasswordChange, // ✅ THÊM
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    LoginCount = u.LoginCount
                }).ToList();

            return Ok(users);
        }

        // ✅ Thống kê user
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetStatistics()
        {
            var stats = new
            {
                TotalUsers = _context.Users.Count(u => !u.IsDeleted),
                TotalAdmin = _context.Users.Count(u => !u.IsDeleted && u.Role == "Admin"),
                TotalSales = _context.Users.Count(u => !u.IsDeleted && u.Role == "Sales"),
                TotalWarehouse = _context.Users.Count(u => !u.IsDeleted && u.Role == "Warehouse"),
                TotalLocked = _context.Users.Count(u => !u.IsDeleted && u.IsLocked),
                TotalDeleted = _context.Users.Count(u => u.IsDeleted)
            };

            return Ok(stats);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetById(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id && !u.IsDeleted);
            if (user == null)
                return NotFound();

            return Ok(new UserResponseDTO
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Avatar = user.Avatar,
                Role = user.Role,
                IsLocked = user.IsLocked,
                RequirePasswordChange = user.RequirePasswordChange, // ✅ THÊM
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                LoginCount = user.LoginCount
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(CreateUserDTO dto)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Username == dto.Username && !u.IsDeleted);
            if (existingUser != null)
                return BadRequest(new { message = "Username đã tồn tại" });

            var user = new User
            {
                Username = dto.Username,
                Password = dto.Password,
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Avatar = dto.Avatar,
                Role = dto.Role,
                IsDeleted = false,
                IsLocked = false,
                RequirePasswordChange = true, // ✅ THÊM - BẮT BUỘC YÊU CẦU ĐỔI MẬT KHẨU
                CreatedAt = DateTime.Now,
                LoginCount = 0
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new UserResponseDTO
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Avatar = user.Avatar,
                Role = user.Role,
                IsLocked = user.IsLocked,
                RequirePasswordChange = user.RequirePasswordChange, // ✅ THÊM
                CreatedAt = user.CreatedAt
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, UpdateUserDTO dto)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == id && !u.IsDeleted);
                if (user == null)
                    return NotFound(new { message = "Không tìm thấy user" });

                if (!string.IsNullOrEmpty(dto.FullName))
                    user.FullName = dto.FullName;

                if (!string.IsNullOrEmpty(dto.Password))
                    user.Password = dto.Password;

                if (!string.IsNullOrEmpty(dto.Role))
                    user.Role = dto.Role;

                if (!string.IsNullOrEmpty(dto.Email))
                    user.Email = dto.Email;

                if (!string.IsNullOrEmpty(dto.Phone))
                    user.Phone = dto.Phone;

                if (!string.IsNullOrEmpty(dto.Avatar))
                    user.Avatar = dto.Avatar;

                user.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                return Ok(new UserResponseDTO
                {
                    Id = user.Id,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Avatar = user.Avatar,
                    Role = user.Role,
                    IsLocked = user.IsLocked,
                    RequirePasswordChange = user.RequirePasswordChange, // ✅ THÊM
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ✅ KHÓA TÀI KHOẢN
        [HttpPost("{id}/lock")]
        [Authorize(Roles = "Admin")]
        public IActionResult LockUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id && !u.IsDeleted);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy user" });

            user.IsLocked = true;
            user.LockedAt = DateTime.Now;
            _context.SaveChanges();

            return Ok(new { message = "Tài khoản đã bị khóa" });
        }

        // ✅ MỞ KHÓA TÀI KHOẢN
        [HttpPost("{id}/unlock")]
        [Authorize(Roles = "Admin")]
        public IActionResult UnlockUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id && !u.IsDeleted);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy user" });

            user.IsLocked = false;
            user.LockedAt = null;
            _context.SaveChanges();

            return Ok(new { message = "Tài khoản đã được mở khóa" });
        }

        // ✅ XÓA MỀM (chuyển vào thùng rác)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult SoftDelete(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id && !u.IsDeleted);
            if (user == null)
                return NotFound();

            user.IsDeleted = true;
            user.DeletedAt = DateTime.Now;
            _context.SaveChanges();

            return Ok(new { message = "User đã được chuyển vào thùng rác" });
        }

        // ✅ KHÔI PHỤC USER TỪ THÙNG RÁC
        [HttpPost("{id}/restore")]
        [Authorize(Roles = "Admin")]
        public IActionResult Restore(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id && u.IsDeleted);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy user trong thùng rác" });

            user.IsDeleted = false;
            user.DeletedAt = null;
            _context.SaveChanges();

            return Ok(new { message = "User đã được khôi phục thành công" });
        }

        // ✅ XÓA CỨNG (xóa vĩnh viễn khỏi database)
        [HttpDelete("{id}/permanent")]
        [Authorize(Roles = "Admin")]
        public IActionResult PermanentDelete(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id && u.IsDeleted);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy user trong thùng rác" });

            _context.Users.Remove(user);
            _context.SaveChanges();

            return Ok(new { message = "User đã bị xóa vĩnh viễn" });
        }

        // ✅ KHÔI PHỤC TẤT CẢ user trong thùng rác
        [HttpPost("restore-all")]
        [Authorize(Roles = "Admin")]
        public IActionResult RestoreAll()
        {
            var deletedUsers = _context.Users.Where(u => u.IsDeleted).ToList();
            foreach (var user in deletedUsers)
            {
                user.IsDeleted = false;
                user.DeletedAt = null;
            }
            _context.SaveChanges();

            return Ok(new { message = $"Đã khôi phục {deletedUsers.Count} user" });
        }

        // ✅ XÓA TẤT CẢ user trong thùng rác vĩnh viễn
        [HttpDelete("empty-trash")]
        [Authorize(Roles = "Admin")]
        public IActionResult EmptyTrash()
        {
            var deletedUsers = _context.Users.Where(u => u.IsDeleted).ToList();
            _context.Users.RemoveRange(deletedUsers);
            _context.SaveChanges();

            return Ok(new { message = $"Đã xóa vĩnh viễn {deletedUsers.Count} user khỏi thùng rác" });
        }

        // ✅ ĐỔI MẬT KHẨU (CHO CHÍNH USER ĐANG ĐĂNG NHẬP)
        [HttpPost("change-password")]
        public IActionResult ChangePassword(ChangePasswordDTO dto)
        {
            try
            {
                // Lấy userId từ token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized(new { message = "Không xác định được người dùng" });

                var userId = int.Parse(userIdClaim.Value);
                var user = _context.Users.FirstOrDefault(u => u.Id == userId && !u.IsDeleted);

                if (user == null)
                    return NotFound(new { message = "Không tìm thấy người dùng" });

                // Kiểm tra mật khẩu cũ
                if (user.Password != dto.OldPassword)
                    return BadRequest(new { message = "Mật khẩu cũ không đúng!" });

                // Kiểm tra mật khẩu mới
                if (string.IsNullOrEmpty(dto.NewPassword) || dto.NewPassword.Length < 6)
                    return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 6 ký tự!" });

                // Kiểm tra ký tự đặc biệt
                if (!HasSpecialCharacter(dto.NewPassword))
                    return BadRequest(new { message = "Mật khẩu mới phải chứa ít nhất 1 ký tự đặc biệt (@, #, $, %, !, &, *)!" });

                // Kiểm tra mật khẩu mới và xác nhận
                if (dto.NewPassword != dto.ConfirmNewPassword)
                    return BadRequest(new { message = "Mật khẩu xác nhận không khớp!" });

                // Kiểm tra mật khẩu mới không trùng mật khẩu cũ
                if (dto.OldPassword == dto.NewPassword)
                    return BadRequest(new { message = "Mật khẩu mới không được trùng với mật khẩu cũ!" });

                // Cập nhật mật khẩu
                user.Password = dto.NewPassword;
                user.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                return Ok(new { message = "Đổi mật khẩu thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Hàm kiểm tra ký tự đặc biệt
        private bool HasSpecialCharacter(string password)
        {
            string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
            return password.Any(c => specialChars.Contains(c));
        }
    }
}