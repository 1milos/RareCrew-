
namespace RareCrew.Core.Entities
{
    public class Employee
    {
        public string Id { get; set; } = null!;
        public string? EmployeeName { get; set; }
        public DateTime StarTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public string? EntryNotes { get; set; }
        public DateTime? DeleteOn { get; set; }
    }
}
