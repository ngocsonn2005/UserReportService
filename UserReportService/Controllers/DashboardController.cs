using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserReportService.Services;

namespace UserReportService.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IReportService _reportService;

        public DashboardController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _reportService.GetDashboardStats();
            return Ok(stats);
        }

        [HttpGet("revenue-by-month")]
        public async Task<IActionResult> GetRevenueByMonth(int year = 2024)
        {
            var data = await _reportService.GetRevenueByMonth(year);
            return Ok(data);
        }

        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts(int top = 5)
        {
            var data = await _reportService.GetTopProducts(top);
            return Ok(data);
        }
    }
}