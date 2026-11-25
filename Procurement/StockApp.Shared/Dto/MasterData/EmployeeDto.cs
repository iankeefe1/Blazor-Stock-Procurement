using System.ComponentModel.DataAnnotations;

namespace StockApp.Shared.Dtos
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        [Required] public string Nik { get; set; } = "";
        [Required] public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Position { get; set; } = "";
        public string Warehouse { get; set; } = "";
        public string Role { get; set; } = "";
        public bool IsActive { get; set; } = true;

        public EmployeeDto Clone() => (EmployeeDto)MemberwiseClone();
    }
}