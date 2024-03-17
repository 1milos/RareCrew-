using Microsoft.AspNetCore.Mvc;
using RareCrew.Services.Interfaces;
using RareCrew.Services.ViewModel;


namespace RareCrew.Controllers
{
    public class EmployeeController : Controller
    {

        private readonly IEmployeeService _employeeService;
        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }



        [HttpGet]
        [Route("employees")]
        public async Task<IActionResult> GetEmployees()
        {
            try
            {
                var employees = await _employeeService.GetEmployeesAsync();
                var pieChartBytes = await _employeeService.GeneratePieChart(employees);
                return View(new EmployeesViewModel { Employees = employees, PieChartBytes = pieChartBytes });
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
