namespace UserReportService.Services
{
    public class DashboardStatsDTO
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalStock { get; set; }      // ✅ Thêm tồn kho
        public int LowStockCount { get; set; }
        public int TotalCustomers { get; set; }
    }

    public class MonthlyRevenueDTO
    {
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class TopProductDTO
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
    }
}