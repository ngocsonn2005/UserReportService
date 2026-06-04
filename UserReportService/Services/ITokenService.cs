using UserReportService.Models;

namespace UserReportService.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}