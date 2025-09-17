using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace CarRentalManagementSystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext? _context;
    private readonly IWebHostEnvironment? _env;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext? context = null, IWebHostEnvironment? env = null)
    {
        _logger = logger;
        _context = context;
        _env = env;
    }

    public async Task<IActionResult> Index(string? vehicleType = null)
    {
        try
        {
            if (_context != null)
            {
                var query = _context.Vehicles.AsQueryable();
                
                // Filter by vehicle type if specified
                if (!string.IsNullOrEmpty(vehicleType))
                {
                    query = query.Where(v => v.VehicleType.ToLower() == vehicleType.ToLower());
                }
                
                var vehicles = await query
                    .OrderByDescending(v => v.CreatedAt)
                    .Take(6) // Show only 6 latest vehicles on home page
                    .ToListAsync();

                var vehicleViewModels = vehicles.Select(v => new VehicleViewModel
                {
                    Id = v.Id,
                    VehicleType = v.VehicleType,
                    Model = v.Model,
                    Year = v.Year,
                    RegistrationNumber = v.RegistrationNumber,
                    ChassisNumber = v.ChassisNumber,
                    Color = v.Color,
                    EngineCapacity = v.EngineCapacity,
                    FuelType = v.FuelType,
                    DailyRate = v.DailyRate,
                    Seats = v.Seats,
                    Status = v.Status,
                    ImageUrl = GetDefaultImageUrl(v.VehicleType, v.ImageUrl),
                    Description = v.Description,
                    Features = string.IsNullOrEmpty(v.Features) ? new List<string>() : v.Features.Split(',').ToList(),
                    CreatedAt = v.CreatedAt
                }).ToList();

                ViewBag.FeaturedVehicles = vehicleViewModels;
                ViewBag.SelectedVehicleType = vehicleType; // Pass the selected type to the view
            }
            else
            {
                ViewBag.FeaturedVehicles = new List<VehicleViewModel>();
                ViewBag.SelectedVehicleType = vehicleType;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving featured vehicles");
            ViewBag.FeaturedVehicles = new List<VehicleViewModel>();
            ViewBag.SelectedVehicleType = vehicleType;
        }
        
        return View("~/Views/Home/Index.cshtml");
    }

    [HttpDelete]
    [Route("Home/DeleteVehicle/{id}")]
    public async Task<IActionResult> DeleteVehicle(int id)
    {
        try
        {
            if (_context == null)
            {
                return StatusCode(500, new { success = false, message = "Database context not available" });
            }

            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null)
            {
                return NotFound(new { success = false, message = "Vehicle not found" });
            }

            // Attempt to delete the image file only if it is a local upload under wwwroot/uploads/vehicles
            try
            {
                if (!string.IsNullOrWhiteSpace(vehicle.ImageUrl) && _env != null)
                {
                    // Accept both relative "/uploads/vehicles/..." and absolute paths that map under webroot
                    var imagePath = vehicle.ImageUrl.Replace("\\", "/");
                    if (imagePath.StartsWith("/uploads/vehicles/", StringComparison.OrdinalIgnoreCase))
                    {
                        var fullPath = Path.Combine(_env.WebRootPath, imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }
                }
            }
            catch (Exception fileEx)
            {
                // Log and continue; failing to delete a file should not block DB deletion
                _logger.LogWarning(fileEx, "Failed to delete vehicle image for vehicle {VehicleId}", id);
            }

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Vehicle deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vehicle {VehicleId}", id);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // Fallback POST endpoint for environments that block HTTP DELETE from browsers
    [HttpPost]
    [Route("Home/DeleteVehicle")]
    public async Task<IActionResult> DeleteVehiclePost([FromForm] int id)
    {
        return await DeleteVehicle(id);
    }

    [HttpPut]
    [Route("Home/UpdateVehicle")]
    public async Task<IActionResult> UpdateVehicle([FromBody] VehicleViewModel vehicleData)
    {
        try
        {
            if (_context == null)
            {
                return StatusCode(500, new { success = false, message = "Database context not available" });
            }

            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleData.Id);
            if (vehicle == null)
            {
                return NotFound(new { success = false, message = "Vehicle not found" });
            }

            // Check if registration number already exists for other vehicles
            var existingVehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.RegistrationNumber == vehicleData.RegistrationNumber && v.Id != vehicleData.Id);
            
            if (existingVehicle != null)
            {
                return BadRequest(new { success = false, message = "Vehicle with this registration number already exists" });
            }

            // Check if chassis number already exists for other vehicles
            existingVehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.ChassisNumber == vehicleData.ChassisNumber && v.Id != vehicleData.Id);
            
            if (existingVehicle != null)
            {
                return BadRequest(new { success = false, message = "Vehicle with this chassis number already exists" });
            }

            // Update vehicle properties
            vehicle.VehicleType = vehicleData.VehicleType;
            vehicle.Model = vehicleData.Model;
            vehicle.Year = vehicleData.Year;
            vehicle.RegistrationNumber = vehicleData.RegistrationNumber;
            vehicle.ChassisNumber = vehicleData.ChassisNumber;
            vehicle.Color = vehicleData.Color;
            vehicle.EngineCapacity = vehicleData.EngineCapacity;
            vehicle.FuelType = vehicleData.FuelType;
            vehicle.DailyRate = vehicleData.DailyRate;
            vehicle.Seats = vehicleData.Seats;
            vehicle.Status = vehicleData.Status;
            vehicle.Description = vehicleData.Description;
            vehicle.Features = string.Join(",", vehicleData.Features);

            // Update image URL if provided
            if (!string.IsNullOrEmpty(vehicleData.ImageUrl))
            {
                vehicle.ImageUrl = vehicleData.ImageUrl;
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Vehicle updated successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle {VehicleId}", vehicleData.Id);
            return StatusCode(500, new { success = false, message = "Error updating vehicle: " + ex.Message });
        }
    }

    public IActionResult Dashboard()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var userName = HttpContext.Session.GetString("UserName");
        ViewBag.UserName = userName;
        
        return View("~/Views/Manager/Dashboard.cshtml");
    }

    public IActionResult AdminDashboard()
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        
        if (string.IsNullOrEmpty(userId) || userRole != "Admin")
        {
            return RedirectToAction("Login", "Account");
        }

        var userName = HttpContext.Session.GetString("UserName");
        ViewBag.UserName = userName;
        ViewBag.UserRole = userRole;
        
        return View("~/Views/Admin/AdminDashboard.cshtml");
    }

    public IActionResult ManagerDashboard()
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        
        if (string.IsNullOrEmpty(userId) || userRole != "Manager")
        {
            return RedirectToAction("Login", "Account");
        }

        var userName = HttpContext.Session.GetString("UserName");
        ViewBag.UserName = userName;
        ViewBag.UserRole = userRole;
        
        return View("~/Views/Manager/ManagerDashboard.cshtml");
    }

    public IActionResult CustomerDashboard()
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        
        if (string.IsNullOrEmpty(userId) || userRole != "Customer")
        {
            return RedirectToAction("Login", "Account");
        }

        var userName = HttpContext.Session.GetString("UserName");
        ViewBag.UserName = userName;
        ViewBag.UserRole = userRole;
        
        return View("~/Views/Customer/CustomerDashboard.cshtml");
    }

    public IActionResult MyBookings()
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (string.IsNullOrEmpty(userId) || userRole != "Customer")
        {
            return RedirectToAction("Login", "Account");
        }

        var userName = HttpContext.Session.GetString("UserName");
        ViewBag.UserName = userName;
        ViewBag.UserRole = userRole;

        return View("~/Views/Customer/MyBookings.cshtml");
    }

    public IActionResult RentalHistory()
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (string.IsNullOrEmpty(userId) || userRole != "Customer")
        {
            return RedirectToAction("Login", "Account");
        }

        var userName = HttpContext.Session.GetString("UserName");
        ViewBag.UserName = userName;
        ViewBag.UserRole = userRole;

        return View("~/Views/Customer/RentalHistory.cshtml");
    }

    public async Task<IActionResult> Profile()
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        
        if (string.IsNullOrEmpty(userId) || userRole != "Customer")
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            if (_context == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var profileViewModel = new ProfileViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                ProfileImageUrl = user.ProfileImageUrl,
                PreferredCarType = user.PreferredCarType,
                DrivingLicenseImageUrl = user.DrivingLicenseImageUrl,
                NidImageUrl = user.NidImageUrl,
                CarNumber = user.CarNumber,
                DrivingExperienceYears = user.DrivingExperienceYears,
                LicenseNumber = user.LicenseNumber,
                CreatedAt = user.CreatedAt
            };

            return View("~/Views/Customer/Profile.cshtml", profileViewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            return RedirectToAction("Login", "Account");
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile([FromBody] ProfileViewModel profileData)
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        
        if (string.IsNullOrEmpty(userId) || userRole != "Customer")
        {
            return Json(new { success = false, message = "Unauthorized access" });
        }

        try
        {
            if (_context == null)
            {
                return Json(new { success = false, message = "Database context not available" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Update user profile
            user.FirstName = profileData.FirstName;
            user.LastName = profileData.LastName;
            user.PhoneNumber = profileData.PhoneNumber;
            user.Address = profileData.Address;
            user.ProfileImageUrl = profileData.ProfileImageUrl;
            user.PreferredCarType = profileData.PreferredCarType;
            user.DrivingLicenseImageUrl = profileData.DrivingLicenseImageUrl;
            user.NidImageUrl = profileData.NidImageUrl;
            user.CarNumber = profileData.CarNumber;
            user.DrivingExperienceYears = profileData.DrivingExperienceYears;
            user.LicenseNumber = profileData.LicenseNumber;

            await _context.SaveChangesAsync();

            // Update session data
            HttpContext.Session.SetString("UserName", user.FullName);

            return Json(new { success = true, message = "Profile updated successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return Json(new { success = false, message = "Error updating profile: " + ex.Message });
        }
    }

    public async Task<IActionResult> AvailableCars()
    {
        try
        {
            if (_context == null)
            {
                ViewBag.Vehicles = new List<VehicleViewModel>();
                return View("~/Views/Home/AvailableCars.cshtml");
            }

            // Show ALL vehicles regardless of status, but order by status (Available first) then by creation date
            var vehicles = await _context.Vehicles
                .OrderBy(v => v.Status != "Available" ? 1 : 0) // Available vehicles first
                .ThenByDescending(v => v.CreatedAt)
                .ToListAsync();

            var vehicleViewModels = vehicles.Select(v => new VehicleViewModel
            {
                Id = v.Id,
                VehicleType = v.VehicleType,
                Model = v.Model,
                Year = v.Year,
                RegistrationNumber = v.RegistrationNumber,
                ChassisNumber = v.ChassisNumber,
                Color = v.Color,
                EngineCapacity = v.EngineCapacity,
                FuelType = v.FuelType,
                DailyRate = v.DailyRate,
                Seats = v.Seats,
                Status = v.Status,
                ImageUrl = GetDefaultImageUrl(v.VehicleType, v.ImageUrl),
                Description = v.Description,
                Features = string.IsNullOrEmpty(v.Features) ? new List<string>() : v.Features.Split(',').ToList(),
                CreatedAt = v.CreatedAt
            }).ToList();

            ViewBag.Vehicles = vehicleViewModels;
            return View("~/Views/Home/AvailableCars.cshtml");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicles");
            ViewBag.Vehicles = new List<VehicleViewModel>();
            return View("~/Views/Home/AvailableCars.cshtml");
        }
    }

    public IActionResult FleetManagement()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var userName = HttpContext.Session.GetString("UserName");
        ViewBag.UserName = userName;
        
        return View("~/Views/Manager/FleetManagement.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> AddVehicle([FromBody] VehicleViewModel vehicleData)
    {
        try
        {
            if (_context == null)
            {
                return Json(new { success = false, message = "Database context not available" });
            }

            // Check if registration number already exists
            var existingVehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.RegistrationNumber == vehicleData.RegistrationNumber);
            
            if (existingVehicle != null)
            {
                return Json(new { success = false, message = "Vehicle with this registration number already exists" });
            }

            // Check if chassis number already exists
            existingVehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.ChassisNumber == vehicleData.ChassisNumber);
            
            if (existingVehicle != null)
            {
                return Json(new { success = false, message = "Vehicle with this chassis number already exists" });
            }

            // Create new vehicle
            var vehicle = new Vehicle
            {
                VehicleType = vehicleData.VehicleType,
                Model = vehicleData.Model,
                Year = vehicleData.Year,
                RegistrationNumber = vehicleData.RegistrationNumber,
                ChassisNumber = vehicleData.ChassisNumber,
                Color = vehicleData.Color,
                EngineCapacity = vehicleData.EngineCapacity,
                FuelType = vehicleData.FuelType,
                DailyRate = vehicleData.DailyRate,
                Seats = vehicleData.Seats,
                Status = "Available",
                ImageUrl = vehicleData.ImageUrl,
                Description = vehicleData.Description,
                Features = string.Join(",", vehicleData.Features),
                CreatedAt = DateTime.Now
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Vehicle added successfully!", vehicleId = vehicle.Id, redirectUrl = Url.Action("AvailableCars", "Home") });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding vehicle");
            return Json(new { success = false, message = "Error adding vehicle: " + ex.Message });
        }
    }

    public async Task<IActionResult> ViewAllVehicles()
    {
        try
        {
            if (_context == null)
            {
                ViewBag.Vehicles = new List<VehicleViewModel>();
                return View("~/Views/Manager/ViewAllVehicles.cshtml");
            }

            var vehicles = await _context.Vehicles
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            var vehicleViewModels = vehicles.Select(v => new VehicleViewModel
            {
                Id = v.Id,
                VehicleType = v.VehicleType,
                Model = v.Model,
                Year = v.Year,
                RegistrationNumber = v.RegistrationNumber,
                ChassisNumber = v.ChassisNumber,
                Color = v.Color,
                EngineCapacity = v.EngineCapacity,
                FuelType = v.FuelType,
                DailyRate = v.DailyRate,
                Seats = v.Seats,
                Status = v.Status,
                ImageUrl = GetDefaultImageUrl(v.VehicleType, v.ImageUrl),
                Description = v.Description,
                Features = string.IsNullOrEmpty(v.Features) ? new List<string>() : v.Features.Split(',').ToList(),
                CreatedAt = v.CreatedAt
            }).ToList();

            ViewBag.Vehicles = vehicleViewModels;
            return View("~/Views/Manager/ViewAllVehicles.cshtml");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicles");
            ViewBag.Vehicles = new List<VehicleViewModel>();
            return View("~/Views/Manager/ViewAllVehicles.cshtml");
        }
    }

    public IActionResult FinesManagement()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var userName = HttpContext.Session.GetString("UserName");
        ViewBag.UserName = userName;
        
        return View("~/Views/Manager/FinesManagement.cshtml");
    }

    public IActionResult Services()
    {
        return View("~/Views/Home/Services.cshtml");
    }

    public IActionResult About()
    {
        return View("~/Views/Home/About.cshtml");
    }

    public IActionResult Contact()
    {
        return View("~/Views/Home/Contact.cshtml");
    }

    public IActionResult VehicleTypes()
    {
        return View("~/Views/Home/VehicleTypes.cshtml");
    }

    public async Task<IActionResult> ViewVehiclesByCategory(string category)
    {
        try
        {
            if (_context == null)
            {
                ViewBag.Category = category;
                ViewBag.Vehicles = new List<VehicleViewModel>();
                return View("~/Views/Customer/ViewVehiclesByCategory.cshtml");
            }

            var vehicles = await _context.Vehicles
                .Where(v => v.VehicleType.ToLower() == category.ToLower())
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            var vehicleViewModels = vehicles.Select(v => new VehicleViewModel
            {
                Id = v.Id,
                VehicleType = v.VehicleType,
                Model = v.Model,
                Year = v.Year,
                RegistrationNumber = v.RegistrationNumber,
                ChassisNumber = v.ChassisNumber,
                Color = v.Color,
                EngineCapacity = v.EngineCapacity,
                FuelType = v.FuelType,
                DailyRate = v.DailyRate,
                Seats = v.Seats,
                Status = v.Status,
                ImageUrl = GetDefaultImageUrl(v.VehicleType, v.ImageUrl),
                Description = v.Description,
                Features = string.IsNullOrEmpty(v.Features) ? new List<string>() : v.Features.Split(',').ToList(),
                CreatedAt = v.CreatedAt
            }).ToList();

            ViewBag.Category = category;
            ViewBag.Vehicles = vehicleViewModels;
            return View("~/Views/Customer/ViewVehiclesByCategory.cshtml");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicles by category");
            ViewBag.Category = category;
            ViewBag.Vehicles = new List<VehicleViewModel>();
            return View("~/Views/Customer/ViewVehiclesByCategory.cshtml");
        }
    }

    private string GetDefaultImageUrl(string vehicleType, string currentImageUrl)
    {
        // If there's already an image URL, use it
        if (!string.IsNullOrEmpty(currentImageUrl))
        {
            return currentImageUrl;
        }

        // Return default images based on vehicle type
        return vehicleType.ToLower() switch
        {
            "motor bike" => "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=400&h=300&fit=crop",
            "cng" => "https://images.unsplash.com/photo-1549924231-f129b911e442?w=400&h=300&fit=crop",
            "private car" => "https://images.unsplash.com/photo-1549317661-bd32c8ce0db2?w=400&h=300&fit=crop",
            "pickup" => "https://images.unsplash.com/photo-1582639510494-c80b5de9f148?w=400&h=300&fit=crop",
            "truck" => "https://images.unsplash.com/photo-1566576912321-d58ddd7a6088?w=400&h=300&fit=crop",
            "covered van" => "https://images.unsplash.com/photo-1582639510494-c80b5de9f148?w=400&h=300&fit=crop",
            _ => "https://images.unsplash.com/photo-1549317661-bd32c8ce0db2?w=400&h=300&fit=crop"
        };
    }

    public IActionResult Privacy()
    {
        return View("~/Views/Home/Privacy.cshtml");
    }

    [HttpGet]
    public async Task<IActionResult> GetFleet()
    {
        try
        {
            if (_context == null)
            {
                return Ok(new List<VehicleViewModel>());
            }

            var vehicles = await _context.Vehicles
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            var vehicleViewModels = vehicles.Select(v => new VehicleViewModel
            {
                Id = v.Id,
                VehicleType = v.VehicleType,
                Model = v.Model,
                Year = v.Year,
                RegistrationNumber = v.RegistrationNumber,
                ChassisNumber = v.ChassisNumber,
                Color = v.Color,
                EngineCapacity = v.EngineCapacity,
                FuelType = v.FuelType,
                DailyRate = v.DailyRate,
                Seats = v.Seats,
                Status = v.Status,
                ImageUrl = GetDefaultImageUrl(v.VehicleType, v.ImageUrl),
                Description = v.Description,
                Features = string.IsNullOrEmpty(v.Features) ? new List<string>() : v.Features.Split(',').ToList(),
                CreatedAt = v.CreatedAt
            }).ToList();

            return Ok(vehicleViewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fleet");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UploadVehicleImage(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "No file uploaded" });
            }

            if (_env == null)
            {
                return StatusCode(500, new { success = false, message = "Hosting environment not available" });
            }

            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "vehicles");
            if (!Directory.Exists(uploadsRoot))
            {
                Directory.CreateDirectory(uploadsRoot);
            }

            var safeFileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(file.FileName);
            var fullPath = Path.Combine(uploadsRoot, safeFileName);

            using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/vehicles/{safeFileName}";
            return Ok(new { success = true, url = relativePath });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading vehicle image");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UploadProfileImage(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "No file uploaded" });
            }

            if (_env == null)
            {
                return StatusCode(500, new { success = false, message = "Hosting environment not available" });
            }

            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }

            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "profiles");
            if (!Directory.Exists(uploadsRoot))
            {
                Directory.CreateDirectory(uploadsRoot);
            }

            var safeFileName = $"profile_{userId}_{Path.GetFileNameWithoutExtension(Path.GetRandomFileName())}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(uploadsRoot, safeFileName);

            using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/profiles/{safeFileName}";
            return Ok(new { success = true, url = relativePath });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile image");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> CheckProfileCompletion()
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        
        if (string.IsNullOrEmpty(userId) || userRole != "Customer")
        {
            return Json(new { success = false, message = "Unauthorized access" });
        }

        try
        {
            if (_context == null)
            {
                return Json(new { success = false, message = "Database context not available" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var profileViewModel = new ProfileViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                ProfileImageUrl = user.ProfileImageUrl,
                PreferredCarType = user.PreferredCarType,
                DrivingLicenseImageUrl = user.DrivingLicenseImageUrl,
                NidImageUrl = user.NidImageUrl,
                CarNumber = user.CarNumber,
                DrivingExperienceYears = user.DrivingExperienceYears,
                LicenseNumber = user.LicenseNumber,
                CreatedAt = user.CreatedAt
            };

            return Json(new { 
                success = true, 
                isProfileComplete = profileViewModel.IsProfileComplete,
                isDrivingInfoComplete = profileViewModel.IsDrivingInfoComplete,
                isDocumentsComplete = profileViewModel.IsDocumentsComplete,
                message = profileViewModel.IsProfileComplete ? 
                    "Profile is complete. You can proceed with car rental." : 
                    "Please complete your profile to rent a car. Complete Driving Information and Documents & Images sections."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking profile completion");
            return Json(new { success = false, message = "Error checking profile completion: " + ex.Message });
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
