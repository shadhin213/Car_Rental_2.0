using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string VehicleType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Model { get; set; } = string.Empty;
        
        [Required]
        [Range(1900, 2030)]
        public int Year { get; set; }
        
        [Required]
        [StringLength(20)]
        public string RegistrationNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string ChassisNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(30)]
        public string Color { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string EngineCapacity { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string FuelType { get; set; } = string.Empty;
        
        [Required]
        [Range(0, 10000)]
        public decimal DailyRate { get; set; }
        
        [Range(1, 100)]
        public int Seats { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Available";
        
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Features { get; set; } = string.Empty; // Comma-separated features
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
    }
}

