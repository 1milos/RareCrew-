
using RareCrew.Services.ViewModel;


namespace RareCrew.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<EmployeeViewModel>> GetEmployeesAsync();

    }
}
