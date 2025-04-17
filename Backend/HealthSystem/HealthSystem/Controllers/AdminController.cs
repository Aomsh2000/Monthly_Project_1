using HealthSystem.Data;
using HealthSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using BCrypt.Net;
using OfficeOpenXml;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using HealthSystem.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace HealthSystem.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITwilioService _twilioService;
        private readonly IDistributedCache _cache;

        public AdminController(AppDbContext context, ITwilioService twilioService, IDistributedCache cache)
        {
            _context = context;
            _twilioService = twilioService;
            _cache = cache;
        }


        // ------- Admin & statistics -------

      //  [Authorize(Roles = "Admin")]
        [HttpGet("graph/barChart")]
        public async Task<IActionResult> Barchart()
        {
            var cacheKey = "barChartData";
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return Ok(System.Text.Json.JsonSerializer.Deserialize<object>(cachedData));
            }

            var patientsCount = await _context.Patients.CountAsync();
            var doctorsCount = await _context.Doctors.CountAsync();

            var BarchartData = new
            {
                patients = patientsCount,
                doctors = doctorsCount
            };

            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10)); // Cache duration

            var serializedData = System.Text.Json.JsonSerializer.Serialize(BarchartData);
            await _cache.SetStringAsync(cacheKey, serializedData, options);

            return Ok(BarchartData);
        }

      //  [Authorize(Roles = "Admin")]
        [HttpGet("graph/piechart")]
        public async Task<IActionResult> Piechart()
        {
            var cacheKey = "PiechartData";
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                var data = JsonSerializer.Deserialize<object>(cachedData);
                return Ok(data);
            }

            var maleCount = await _context.Patients.CountAsync(p => p.Gender == Gender.Male);
            var femaleCount = await _context.Patients.CountAsync(p => p.Gender == Gender.Female);

            var PiechartData = new[]
            {
        new { name = "Male", value = maleCount },
        new { name = "Female", value = femaleCount },
    };

            var serializedData = JsonSerializer.Serialize(PiechartData);
            await _cache.SetStringAsync(cacheKey, serializedData, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });

            return Ok(PiechartData);

        }


        // ------- Admin & Patient -------
    //    [Authorize(Roles = "Admin")]
        [HttpPost("create-patient")]
        public async Task<IActionResult> CreatePatient([FromBody] PatientCreateRequest request)
        {
            try
            {
                // Validate the request
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == request.user.email))
                {
                    return BadRequest("Email already exists.");
                }
                // Check if national ID already exists
                if (await _context.Patients.AnyAsync(p => p.NationalID == request.nationalID))
                {
                    return BadRequest("National ID already exists.");
                }
                // Parse date string into year, month, day components
                DateTime dateOfBirth;
                try
                {
                    var dateParts = request.dateOfBirth.Split('-');
                    if (dateParts.Length != 3)
                    {
                        return BadRequest("Invalid date format. Use YYYY-MM-DD.");
                    }

                    int year = int.Parse(dateParts[0]);
                    int month = int.Parse(dateParts[1]);
                    int day = int.Parse(dateParts[2]);

                    dateOfBirth = new DateTime(year, month, day);
                }
                catch
                {
                    return BadRequest("Invalid date format. Use YYYY-MM-DD.");
                }

                // Create the User
                var user = new User
                {
                    UserID = Guid.NewGuid(),
                    FirstName = request.user.firstName,
                    MiddleName = request.user.middleName,
                    LastName = request.user.lastName,
                    Email = request.user.email,
                    PhoneNumber = request.user.phoneNumber,
                    Password = BCrypt.Net.BCrypt.HashPassword(request.user.password),
                    Role = UserRole.Patient
                };
                // Create the Patient
                var patient = new Patient
                {
                    UserID = user.UserID,
                    NationalID = request.nationalID,
                    DateOfBirth = dateOfBirth,
                    Gender = Enum.Parse<Gender>(request.gender),
                    BloodType = Enum.Parse<BloodType>(request.bloodType.Replace("+", "_Positive").Replace("-", "_Negative")),
                    Allergies = request.allergies,
                    ChronicDiseases = request.chronicDiseases
                };
                // Add to database
                _context.Users.Add(user);
                _context.Patients.Add(patient);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Patient created successfully",
                    userId = user.UserID
                });
            }
            catch (Exception)
            {
                throw; //rethrows the exception to allow Bugsnag to log it
            }
        }




        // ------- Admin & Doctor -------




        // ****  Create new Doctor API  *****
  //      [Authorize(Roles = "Admin")]
        [HttpPost("create-doctor")]
        public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorRequest request)
        {
            if (request == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid doctor data.");
            }

            // Step 1: Create a new User
            var user = new User
            {
                UserID = Guid.NewGuid(),
                FirstName = request.FirstName,
                MiddleName = request.MiddleName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.Doctor
            };

            // Add the user to the database
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Step 2: Create a new Doctor
            var doctor = new Doctor
            {
                UserID = user.UserID,
                Gender = Enum.TryParse(request.Gender, out Gender gender) ? gender : Gender.Male, // Default is Male
                Specialization = request.Specialization,
                Clinic = Enum.TryParse(request.Clinic, out ClinicType clinic) ? clinic : ClinicType.General, // Default is General
                User = user,
                WorkingHours = new List<WorkingHours>() // Initialize WorkingHours list
            };

            // Add working hours for the doctor
            foreach (var workingHour in request.WorkingHours)
            {
                var hours = new WorkingHours
                {
                    UserID = doctor.UserID,
                    Day = Enum.TryParse(workingHour.Day, out dayOfWeek day) ? day : dayOfWeek.Monday, // Default to Monday if invalid
                    StartTime = TimeSpan.TryParse(workingHour.StartTime, out TimeSpan startTime) ? startTime : TimeSpan.Zero, // Default to 00:00 if invalid
                    EndTime = TimeSpan.TryParse(workingHour.EndTime, out TimeSpan endTime) ? endTime : TimeSpan.Zero, // Default to 00:00 if invalid
                    Doctor = doctor
                };
                doctor.WorkingHours.Add(hours);
            }

            // Step 3: Save the doctor to the database
            await _context.Doctors.AddAsync(doctor);
            await _context.SaveChangesAsync();

            // Step 4: Return success message
            return Ok(new { message = "Doctor created successfully" });
        }






        // ****  Get All Doctors API  *****
       // [Authorize(Roles = "Admin")]
        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors()
        {
            var cacheKey = "DoctorsWithDetails";
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                var cachedDoctors = JsonSerializer.Deserialize<List<object>>(cachedData);
                return Ok(cachedDoctors);
            }

            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.WorkingHours)
                .ToListAsync();

            var doctorDtos = new List<object>();

            foreach (var doctor in doctors)
            {
                var doctorDto = new
                {
                    user = new
                    {
                        id = doctor.User.UserID,
                        uuid = doctor.User.UserID.ToString(),
                        firstName = doctor.User.FirstName,
                        middleName = doctor.User.MiddleName,
                        lastName = doctor.User.LastName,
                        email = doctor.User.Email,
                        phoneNumber = doctor.User.PhoneNumber,
                        role = doctor.User.Role
                    },
                    gender = doctor.Gender,
                    specialization = doctor.Specialization,
                    clinic = doctor.Clinic,
                    workingHours = doctor.WorkingHours.Select(wh => new
                    {
                        day = wh.Day.ToString().ToUpper(),
                        startTime = wh.StartTime.ToString(@"hh\:mm"),
                        endTime = wh.EndTime.ToString(@"hh\:mm")
                    }).ToList()
                };

                doctorDtos.Add(doctorDto);
            }

            var serializedData = JsonSerializer.Serialize(doctorDtos);
            await _cache.SetStringAsync(cacheKey, serializedData, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });

            return Ok(doctorDtos);
        }






        // **  Create new Appointment API  ***
      //  [Authorize(Roles = "Admin")]
        [HttpPost("appointments/create")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest request)
        {
            // Validate input
            if (request == null || !ModelState.IsValid)
            {
                // Add these debug lines
                return BadRequest("Invalid appointment data.");
            }



            // Step 1: Check if patient and doctor exist
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserID == request.PatientID);
            var doctor = await _context.Doctors.Include(d => d.User)
                                               .Include(d => d.WorkingHours)
                                               .FirstOrDefaultAsync(d => d.UserID == request.DoctorID);

            if (patient == null || doctor == null)
            {
                return NotFound("Patient or Doctor not found.");
            }

            // Step 2: Check if the appointment time is within the doctor's working hours
            var appointmentTime = TimeSpan.Parse(request.AppointmentTime);

            // Step 3: Create the new appointment
            var appointment = new Appointment
            {
                PatientUserID = patient.UserID,
                DoctorUserID = doctor.UserID,
                AppointmentDate = request.AppointmentDate,
                AppointmentTime = appointmentTime,
                Status = AppointmentStatus.Upcoming, // Default "Upcoming"
                Note = ""// empty note by default
            };

            // Save the appointment to the database
            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();

            //Twilio Send SMS
            var fullDateTime = appointment.AppointmentDate.Date + appointment.AppointmentTime; // Combine date and time
            var formattedTime = fullDateTime.ToString("hh:mm tt"); // ✅ Works with AM/PM
            var formattedDate = appointment.AppointmentDate.ToString("yyyy-MM-dd"); // formatting

            var message = $"Dear {patient.User.FirstName} {patient.User.LastName}, Your appointment is confirmed for {formattedTime} on {formattedDate}.";
            // Format patient's phone number
            var patientPhone = patient.User.PhoneNumber;
            if (patientPhone.StartsWith("8"))
            {
                patientPhone = "+1" + patientPhone.Substring(0); // Convert to international format
            }
            try
            {
                // Send SMS using Twilio
                await _twilioService.SendSmsAsync(patientPhone, message);
            }
            catch (Exception ex)
            {
                // Log error or handle it based on your application's error-handling strategy
                return StatusCode(500, "Error sending SMS: " + ex.Message);
            }
            // Return success message
            return Ok(new
            {
                message = "Appointment created successfully"

            });
        }




        // ****  Get download Excel file contain doctor information API  *****
        // GET: /api/admin/download-excel/{doctorId}
        //[Authorize(Roles = "Admin")]
        [HttpGet("download-excel/{doctorId}")]
        public async Task<IActionResult> DownloadDoctorExcel(Guid doctorId)
        {
            // Retrieve the specific doctor by ID
            var doctor = await _context.Doctors
                .Include(d => d.User)  // Include user details like name, email, etc.
                .FirstOrDefaultAsync(d => d.UserID == doctorId);

            if (doctor == null)
            {
                return NotFound("Doctor not found.");
            }
            //Set license context
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            // Create a new Excel package
            using (var package = new ExcelPackage())
            {
                // Add a worksheet to the package
                var worksheet = package.Workbook.Worksheets.Add("Doctor Info");

                // Add headers to the Excel sheet
                worksheet.Cells[1, 1].Value = "Doctor ID";
                worksheet.Cells[1, 2].Value = "First Name";
                worksheet.Cells[1, 3].Value = "Middle Name";
                worksheet.Cells[1, 4].Value = "Last Name";
                worksheet.Cells[1, 5].Value = "Email";
                worksheet.Cells[1, 6].Value = "Phone Number";
                worksheet.Cells[1, 7].Value = "Gender";
                worksheet.Cells[1, 8].Value = "Specialization";
                worksheet.Cells[1, 9].Value = "Clinic";

                // Populate the worksheet with data for the single doctor
                worksheet.Cells[2, 1].Value = doctor.User.UserID;
                worksheet.Cells[2, 2].Value = doctor.User.FirstName;
                worksheet.Cells[2, 3].Value = doctor.User.MiddleName;
                worksheet.Cells[2, 4].Value = doctor.User.LastName;
                worksheet.Cells[2, 5].Value = doctor.User.Email;
                worksheet.Cells[2, 6].Value = doctor.User.PhoneNumber;
                worksheet.Cells[2, 7].Value = doctor.Gender.ToString();
                worksheet.Cells[2, 8].Value = doctor.Specialization;
                worksheet.Cells[2, 9].Value = doctor.Clinic.ToString();

                // Convert the package to a byte array
                var fileBytes = package.GetAsByteArray();

                // Return the Excel file as a download
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{doctor.User.FirstName}_{doctor.User.LastName}_DoctorInfo.xlsx");
            }
        }

        // ****  Get Availabel appointments API  *****
      //  [Authorize(Roles = "Admin")]
        [HttpGet("getAllAvailablAappointments")]
        public async Task<IActionResult> GetAvailableAppointments(DateTime date, ClinicType clinic)
        {
            // Create a unique cache key using both date and clinic
            var cacheKey = $"AvailableAppointments_{clinic}_{date:yyyyMMdd}";
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                var cachedAppointments = JsonSerializer.Deserialize<object>(cachedData);
                return Ok(cachedAppointments);
            }

            var dayOfWeek = date.DayOfWeek;

            var doctorsInClinic = await _context.Doctors
                .Where(d => d.Clinic == clinic)
                .Include(d => d.WorkingHours)
                .Include(d => d.User)
                .ToListAsync();

            var availableAppointments = new List<object>();

            foreach (var doctor in doctorsInClinic)
            {
                var workingHours = doctor.WorkingHours
                    .Where(wh => (DayOfWeek)wh.Day == dayOfWeek)
                    .ToList();

                foreach (var hours in workingHours)
                {
                    var bookedAppointments = await _context.Appointments
                        .Where(a => a.DoctorUserID == doctor.UserID
                                 && a.AppointmentDate.Date == date.Date
                                 && a.Status == AppointmentStatus.Upcoming)
                        .ToListAsync();

                    var availableTimeSlots = GetAvailableTimeSlots(hours, bookedAppointments);

                    if (availableTimeSlots != null)
                    {
                        availableAppointments.Add(new
                        {
                            DoctorName = $"{doctor.User.FirstName} {doctor.User.LastName}",
                            DoctorID = doctor.User.UserID,
                            Specialization = doctor.Specialization,
                            Clinic = doctor.Clinic,
                            AvailableTimeSlots = availableTimeSlots
                        });
                    }
                }
            }

            var result = new { availableAppointments };
            var serializedData = JsonSerializer.Serialize(result);

            await _cache.SetStringAsync(cacheKey, serializedData, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) // You can adjust the duration
            });

            return Ok(result);
        }

        private List<TimeSpan> GetAvailableTimeSlots(WorkingHours workingHours, List<Appointment> bookedAppointments)
        {
            // Generate time slots between the working hours (30-minute intervals)
            List<TimeSpan> availableTimeSlots = new List<TimeSpan>();

            for (var time = workingHours.StartTime; time < workingHours.EndTime; time = time.Add(TimeSpan.FromMinutes(30)))
            {
                // Check if the time slot is already booked
                var isBooked = bookedAppointments
                    .Any(a => a.AppointmentTime == time);

                // If the slot is not booked, add it as an available time slot
                if (!isBooked)
                {
                    availableTimeSlots.Add(time);
                }
            }

            return availableTimeSlots;
        }


        //End of endpoints
    }










    // ------- DTO -------

    // Request model for patient creation
    public class PatientCreateRequest
    {
        public UserRequest user { get; set; }
        public string nationalID { get; set; }
        public string dateOfBirth { get; set; }
        public string gender { get; set; }
        public string bloodType { get; set; }
        public string allergies { get; set; }
        public string chronicDiseases { get; set; }
    }

    public class UserRequest
    {
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
        public string password { get; set; }
    }
    // Define the CreateDoctorRequest class to match input structure for creating new doctor
    public class CreateDoctorRequest
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string Gender { get; set; }
        public string Specialization { get; set; }
        public string Clinic { get; set; }
        public List<WorkingHoursRequest> WorkingHours { get; set; }
    }

    // Define the WorkingHoursRequest class for working hours input 
    public class WorkingHoursRequest
    {
        public string Day { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    // Define the CreateAppointmentRequest class to match input structure for creating new appointment
    public class CreateAppointmentRequest
    {
        public Guid PatientID { get; set; }
        public Guid DoctorID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string AppointmentTime { get; set; }
    }
}