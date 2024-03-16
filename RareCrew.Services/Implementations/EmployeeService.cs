using Newtonsoft.Json;
using RareCrew.Core.Entities;
using RareCrew.Services.Interfaces;
using RareCrew.Services.ViewModel;


namespace RareCrew.Services.Implementations
{
    public class EmployeeService : IEmployeeService
    {
        private readonly HttpClient _httpClient;
        public EmployeeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<EmployeeViewModel>> GetEmployeesAsync()
        {
            try
            {
                const string apiUrl = "https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
                var response = await _httpClient.GetAsync(apiUrl);
                var employeeViewModels = new List<EmployeeViewModel>();

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var employees = JsonConvert.DeserializeObject<List<Employee>>(jsonContent);

                        if (employees == null || employees.Count == 0)
                        {
                            throw new InvalidOperationException("List employees is null or empty.");
                        }

                        var groupedEmployees = employees.Where(emp => !string.IsNullOrEmpty(emp.EmployeeName) || emp.DeleteOn.HasValue).GroupBy(emp => emp.EmployeeName);

                        var totalWorkHoursByEmployee = new Dictionary<string, double>();

                        foreach (var group in groupedEmployees)
                        {
                            double totalWorkHours = 0;
                            foreach (var employee in group)
                            {
                                if (employee.StarTimeUtc >= employee.EndTimeUtc)
                                    continue;

                                var ts = employee.EndTimeUtc - employee.StarTimeUtc;
                                totalWorkHours += ts.TotalHours;
                                
                            }
                            totalWorkHours = Math.Round(totalWorkHours, 2);
                            totalWorkHoursByEmployee.Add(group.Key!, totalWorkHours);
                        }

                        var sortedEmployees = totalWorkHoursByEmployee.OrderByDescending(kvp => kvp.Value);

                        employeeViewModels = sortedEmployees
                            .Select(kvp => new EmployeeViewModel
                            {
                                EmployeeName = kvp.Key,
                                TotalWorkHours = kvp.Value
                            }).ToList();
                    }
                    catch (JsonException)
                    {
                        throw;
                    }
                    return employeeViewModels;
                }
                else
                {
                    throw new HttpRequestException($"Failed to retrieve employees. Status code: {response.StatusCode}");
                }

            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
