using Microsoft.AspNetCore.Mvc;
using RareCrew.Services.Interfaces;


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
                return View(employees);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
