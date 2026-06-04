namespace UserReportService.Services
{
    public interface IReportService
    {
        Task<DashboardStatsDTO> GetDashboardStats();
        Task<List<MonthlyRevenueDTO>> GetRevenueByMonth(int year);
        Task<List<TopProductDTO>> GetTopProducts(int top);
    }
}