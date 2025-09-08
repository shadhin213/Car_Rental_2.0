using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.Models
{
    public class ProfileViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [StringLength(500)]
        [Display(Name = "Profile Image")]
        public string? ProfileImageUrl { get; set; }

        [StringLength(50)]
        [Display(Name = "Preferred Car Type")]
        public string? PreferredCarType { get; set; }

        [StringLength(500)]
        [Display(Name = "Driving License Image")]
        public string? DrivingLicenseImageUrl { get; set; }

        [StringLength(500)]
        [Display(Name = "National ID Image")]
        public string? NidImageUrl { get; set; }

        [StringLength(20)]
        [Display(Name = "Car Number")]
        public string? CarNumber { get; set; }

        [Range(0, 100)]
        [Display(Name = "Driving Experience (Years)")]
        public int? DrivingExperienceYears { get; set; }

        [StringLength(100)]
        [Display(Name = "License Number")]
        public string? LicenseNumber { get; set; }

        [Display(Name = "Member Since")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";
    }
}