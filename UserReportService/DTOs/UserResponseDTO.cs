using System.ComponentModel.DataAnnotations;

namespace UserReportService.DTOs
{
    public class UserResponseDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Avatar { get; set; }
        public string Role { get; set; }
        public bool IsLocked { get; set; }
        public bool RequirePasswordChange { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int LoginCount { get; set; }
    }

    public class CreateUserDTO
    {
        [Required(ErrorMessage = "Username không được để trống")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password không được để trống")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string FullName { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }
        public string Avatar { get; set; }

        [Required(ErrorMessage = "Role không được để trống")]
        public string Role { get; set; }
    }

    public class UpdateUserDTO
    {
        public string FullName { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Avatar { get; set; }
    }
}