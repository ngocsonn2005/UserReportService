using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using UserReportService.Data;

namespace UserReportService.Filters
{
    public class LockCheckFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Bỏ qua kiểm tra cho các endpoint không cần xác thực
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata
                .Any(em => em.GetType() == typeof(AllowAnonymousAttribute));

            if (allowAnonymous)
                return;

            // Lấy userId từ claims
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return;

            var userId = int.Parse(userIdClaim.Value);

            // Lấy DbContext từ service provider
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

            // Kiểm tra user trong database
            var user = dbContext.Users.Find(userId);

            if (user == null || user.IsDeleted)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    message = "Tài khoản đã bị vô hiệu hóa!",
                    isLocked = true
                });
                return;
            }

            if (user.IsLocked)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    message = "Tài khoản đã bị khóa. Vui lòng liên hệ Admin để mở khóa!",
                    isLocked = true
                });
                return;
            }
        }
    }
}