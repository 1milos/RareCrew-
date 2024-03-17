
namespace RareCrew.Services.ViewModel
{
    public class EmployeeViewModel
    {
        public string EmployeeName { get; set; } = null!;
        public double TotalWorkHours { get; set; }
    }

    public class EmployeesViewModel
    {
        public List<EmployeeViewModel> Employees { get; set; } = new List<EmployeeViewModel>();
        public byte[] PieChartBytes { get; set; } = Array.Empty<byte>();
    }
}
