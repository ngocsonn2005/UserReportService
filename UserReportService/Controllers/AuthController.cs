using Microsoft.AspNetCore.Mvc;
using UserReportService.Data;
using UserReportService.Models;
using UserReportService.Services;
using Microsoft.EntityFrameworkCore;
using System;

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

                if (user.IsLocked)
                {
                    return Unauthorized(new { message = "Tài khoản đã bị khóa. Vui lòng liên hệ Admin!" });
                }

                user.LastLoginAt = DateTime.Now;
                user.LoginCount = user.LoginCount + 1;
                user.UpdatedAt = DateTime.Now;

                _context.SaveChanges();

                var token = _tokenService.GenerateToken(user);

                return Ok(new
                {
                    token = token,
                    role = user.Role,
                    fullName = user.FullName,
                    userId = user.Id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
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

                // Kiểm tra mật khẩu
                if (dto.Password != dto.ConfirmPassword)
                {
                    return BadRequest(new { message = "Mật khẩu xác nhận không khớp!" });
                }

                if (string.IsNullOrEmpty(dto.Password) || dto.Password.Length < 6)
                {
                    return BadRequest(new { message = "Mật khẩu phải có ít nhất 6 ký tự!" });
                }

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
                // Lỗi database (thiếu cột, sai kiểu dữ liệu)
                Console.WriteLine($"DB Error: {dbEx.InnerException?.Message ?? dbEx.Message}");
                return StatusCode(500, new { message = "Lỗi database: " + (dbEx.InnerException?.Message ?? dbEx.Message) });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Register Error: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
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
}