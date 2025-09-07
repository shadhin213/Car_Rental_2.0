namespace CarRentalManagementSystem.Models
{
    public class VehicleViewModel
    {
        public int Id { get; set; }
        public string VehicleType { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public string ChassisNumber { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string EngineCapacity { get; set; } = string.Empty;
        public string FuelType { get; set; } = string.Empty;
        public decimal DailyRate { get; set; }
        public int Seats { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

