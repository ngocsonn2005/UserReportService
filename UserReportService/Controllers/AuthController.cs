using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserReportService.Data;
using UserReportService.Models;
using UserReportService.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;

namespace UserReportService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthController(AppDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDTO dto)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(x =>
                    x.Username == dto.Username &&
                    x.Password == dto.Password);

                if (user == null)
                {
                    return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu" });
                }

                if (user.IsDeleted)
                {
                    return Unauthorized(new { message = "Tài khoản đã bị vô hiệu hóa. Vui lòng liên hệ Admin!" });
                }

                // ✅ Kiểm tra tài khoản bị khóa
                if (user.IsLocked)
                {
                    return Unauthorized(new { message = "Tài khoản đã bị khóa. Vui lòng liên hệ Admin để mở khóa!" });
                }

                user.LastLoginAt = DateTime.Now;
                user.LoginCount = user.LoginCount + 1;
                user.UpdatedAt = DateTime.Now;

                _context.SaveChanges();

                var token = _tokenService.GenerateToken(user);

                // ✅ Thêm flag requirePasswordChange vào response
                return Ok(new
                {
                    token = token,
                    role = user.Role,
                    fullName = user.FullName,
                    userId = user.Id,
                    requirePasswordChange = user.RequirePasswordChange // Flag yêu cầu đổi mật khẩu
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // ✅ API đổi mật khẩu bắt buộc (dành cho lần đăng nhập đầu tiên)
        [HttpPost("force-change-password")]
        [Authorize]
        public IActionResult ForceChangePassword(ForceChangePasswordDTO dto)
        {
            try
            {
                // Lấy userId từ token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng!" });
                }

                var userId = int.Parse(userIdClaim.Value);
                var user = _context.Users.Find(userId);

                if (user == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng!" });
                }

                // Kiểm tra mật khẩu mới
                if (string.IsNullOrEmpty(dto.NewPassword) || dto.NewPassword.Length < 6)
                {
                    return BadRequest(new { message = "Mật khẩu phải có ít nhất 6 ký tự!" });
                }

                if (!HasSpecialCharacter(dto.NewPassword))
                {
                    return BadRequest(new { message = "Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt (@, #, $, %, !, &, *, etc.)!" });
                }

                // ✅ Cập nhật mật khẩu và xóa flag yêu cầu đổi mật khẩu
                user.Password = dto.NewPassword;
                user.RequirePasswordChange = false;
                user.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                return Ok(new { message = "Đổi mật khẩu thành công!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ForceChangePassword error: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // ✅ API đổi mật khẩu thông thường (dành cho người dùng đã đăng nhập)
        [HttpPost("change-password")]
        [Authorize]
        public IActionResult ChangePassword(ChangePasswordDTO dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng!" });
                }

                var userId = int.Parse(userIdClaim.Value);
                var user = _context.Users.Find(userId);

                if (user == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng!" });
                }

                // Kiểm tra mật khẩu cũ
                if (user.Password != dto.OldPassword)
                {
                    return BadRequest(new { message = "Mật khẩu cũ không chính xác!" });
                }

                // Kiểm tra mật khẩu mới
                if (string.IsNullOrEmpty(dto.NewPassword) || dto.NewPassword.Length < 6)
                {
                    return BadRequest(new { message = "Mật khẩu phải có ít nhất 6 ký tự!" });
                }

                if (!HasSpecialCharacter(dto.NewPassword))
                {
                    return BadRequest(new { message = "Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt (@, #, $, %, !, &, *, etc.)!" });
                }

                // Cập nhật mật khẩu
                user.Password = dto.NewPassword;
                user.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                return Ok(new { message = "Đổi mật khẩu thành công!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ChangePassword error: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // ✅ API khóa tài khoản
        [HttpPost("lock/{id}")]
        [Authorize]
        public IActionResult LockUser(int id)
        {
            try
            {
                // Kiểm tra quyền admin
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (currentUserRole != "Admin")
                {
                    return Forbid("Chỉ Admin mới có quyền khóa tài khoản!");
                }

                var user = _context.Users.Find(id);
                if (user == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng!" });
                }

                if (user.IsLocked)
                {
                    return BadRequest(new { message = "Tài khoản đã bị khóa!" });
                }

                user.IsLocked = true;
                user.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                return Ok(new { message = $"Đã khóa tài khoản {user.Username}!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LockUser error: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // ✅ API mở khóa tài khoản
        [HttpPost("unlock/{id}")]
        [Authorize]
        public IActionResult UnlockUser(int id)
        {
            try
            {
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (currentUserRole != "Admin")
                {
                    return Forbid("Chỉ Admin mới có quyền mở khóa tài khoản!");
                }

                var user = _context.Users.Find(id);
                if (user == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng!" });
                }

                if (!user.IsLocked)
                {
                    return BadRequest(new { message = "Tài khoản chưa bị khóa!" });
                }

                user.IsLocked = false;
                user.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                return Ok(new { message = $"Đã mở khóa tài khoản {user.Username}!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UnlockUser error: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterDTO dto)
        {
            try
            {
                Console.WriteLine($"=== ĐĂNG KÝ ===");
                Console.WriteLine($"Username: {dto.Username}");
                Console.WriteLine($"FullName: {dto.FullName}");

                // Kiểm tra username
                var existingUser = _context.Users.FirstOrDefault(x => x.Username == dto.Username);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Tên đăng nhập đã tồn tại!" });
                }

                // Kiểm tra mật khẩu xác nhận
                if (dto.Password != dto.ConfirmPassword)
                {
                    return BadRequest(new { message = "Mật khẩu xác nhận không khớp!" });
                }

                // Kiểm tra độ dài mật khẩu
                if (string.IsNullOrEmpty(dto.Password) || dto.Password.Length < 6)
                {
                    return BadRequest(new { message = "Mật khẩu phải có ít nhất 6 ký tự!" });
                }

                // ✅ KIỂM TRA KÝ TỰ ĐẶC BIỆT
                if (!HasSpecialCharacter(dto.Password))
                {
                    return BadRequest(new { message = "Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt (@, #, $, %, !, &, *, etc.)!" });
                }

                // Kiểm tra họ tên
                if (string.IsNullOrEmpty(dto.FullName))
                {
                    return BadRequest(new { message = "Vui lòng nhập họ tên!" });
                }

                // Kiểm tra email (nếu có)
                if (!string.IsNullOrEmpty(dto.Email))
                {
                    var existingEmail = _context.Users.FirstOrDefault(x => x.Email == dto.Email);
                    if (existingEmail != null)
                    {
                        return BadRequest(new { message = "Email đã được sử dụng!" });
                    }
                }

                // Tạo user mới
                var user = new User
                {
                    Username = dto.Username,
                    Password = dto.Password,
                    FullName = dto.FullName,
                    Email = dto.Email ?? "",
                    Phone = dto.Phone ?? "",
                    Role = "User",
                    IsDeleted = false,
                    IsLocked = false,
                    RequirePasswordChange = false, // ✅ Mặc định false cho đăng ký thông thường
                    CreatedAt = DateTime.Now,
                    LoginCount = 0
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                Console.WriteLine($"Đăng ký thành công! UserId: {user.Id}");

                // Tạo token và đăng nhập
                var token = _tokenService.GenerateToken(user);

                return Ok(new
                {
                    token = token,
                    role = user.Role,
                    fullName = user.FullName,
                    userId = user.Id,
                    message = "Đăng ký thành công!"
                });
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"DB Error: {dbEx.InnerException?.Message ?? dbEx.Message}");
                return StatusCode(500, new { message = "Lỗi database: " + (dbEx.InnerException?.Message ?? dbEx.Message) });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Register Error: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // ✅ Hàm kiểm tra ký tự đặc biệt
        private bool HasSpecialCharacter(string password)
        {
            string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
            return password.Any(c => specialChars.Contains(c));
        }
    }

    public class LoginDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RegisterDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    // ✅ Thêm các DTO mới
    public class ForceChangePasswordDTO
    {
        public string NewPassword { get; set; }
    }

    public class ChangePasswordDTO
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; internal set; }
    }
}