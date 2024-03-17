using Newtonsoft.Json;
using RareCrew.Core.Entities;
using RareCrew.Services.Interfaces;
using RareCrew.Services.ViewModel;
using System.Drawing;
using System.Drawing.Imaging;



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

        public async Task<byte[]> GeneratePieChart(List<EmployeeViewModel> employees)
        {
            using (var bitmap = new Bitmap(800, 600))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                var backgroundColor = Color.White;
                var textColor = Color.Black;
                var pieFont = new Font(FontFamily.GenericSansSerif, 10);

                graphics.Clear(backgroundColor);

                double totalWorkHours = employees.Sum(emp => emp.TotalWorkHours);

                var dataPoints = employees.Select((emp, index) =>
                    new { Label = emp.EmployeeName, Value = emp.TotalWorkHours, Percentage = emp.TotalWorkHours / totalWorkHours * 100, Color = GetRandomColor() }).ToList();

                var pieRectangle = new Rectangle(100, 100, 400, 400);
                var startAngle = 0.0f;
                foreach (var dataPoint in dataPoints)
                {
                    var sweepAngle = (float)(dataPoint.Value / totalWorkHours * 360);
                    var pieBrush = new SolidBrush(dataPoint.Color);
                    graphics.FillPie(pieBrush, pieRectangle, startAngle, sweepAngle);
                    startAngle += sweepAngle;

                    var midAngle = startAngle - sweepAngle / 2;
                    var labelX = pieRectangle.X + pieRectangle.Width / 2 + (int)(Math.Cos(midAngle * Math.PI / 180) * (pieRectangle.Width / 2 + 20));
                    var labelY = pieRectangle.Y + pieRectangle.Height / 2 + (int)(Math.Sin(midAngle * Math.PI / 180) * (pieRectangle.Height / 2 + 20));

                    var stringFormat = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };

                    var displayText = $"{dataPoint.Label}\n{dataPoint.Percentage:0.00}%";

                    graphics.DrawString(displayText, pieFont, new SolidBrush(textColor), labelX, labelY, stringFormat);
                }

                using (var memoryStream = new MemoryStream())
                {
                    bitmap.Save(memoryStream, ImageFormat.Png);
                    return memoryStream.ToArray();
                }
            }
        }

       
        private Color GetRandomColor()
        {
            var random = new Random();
            return Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
        }

    }
}
