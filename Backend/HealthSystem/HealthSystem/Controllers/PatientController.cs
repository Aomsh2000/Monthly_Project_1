using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authorization;
using HealthSystem.Data;
using HealthSystem.Models;

namespace HealthSystem.Controllers
{
    [Route("api/patients")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly AppDbContext _context;

        public PatientController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // 1. GET patient data by UserID
        //[Authorize(Roles = "Patient")]
        [HttpGet("getPatientData/{userId}")]
        public async Task<IActionResult> GetPatientData(Guid userId)
        {
            if (!_cache.TryGetValue($"user{userId}", out User user))
            {
                // Simulate slow database response
                Thread.Sleep(5000);
                user = await _context.Users
                    .Include(u => u.Patient)
                    .FirstOrDefaultAsync(u => u.UserID == userId);

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

                _cache.Set($"user{userId}", user, cacheOptions);
            }

            var patientData = new
            {
                User = new
                {
                    user.UserID,
                    user.FirstName,
                    user.MiddleName,
                    user.LastName,
                    user.Email,
                    user.PhoneNumber,
                    role = user.Role.ToString()
                },
                Patient = new
                {
                    user.Patient.NationalID,
                    user.Patient.DateOfBirth,
                    Age = (int)((DateTime.Today - user.Patient.DateOfBirth).TotalDays / 365.25),
                    Gender = user.Patient.Gender.ToString(),
                    BloodType = user.Patient.BloodType.ToString(),
                    user.Patient.Allergies,
                    user.Patient.ChronicDiseases
                }
            };

            return Ok(patientData);
        }

        // 2. GET all patient's appointments by UserID
        //[Authorize(Roles = "Patient")]
        [HttpGet("getAppointments/{userId}")]
        public async Task<IActionResult> GetAppointments(Guid userId)
        {
            if (!_cache.TryGetValue($"appointments{userId}", out Patient patient))
            {
                // Simulate slow database response
                Thread.Sleep(5000);
                patient = await _context.Patients
                    .Include(p => p.Appointments)
                        .ThenInclude(a => a.Doctor)
                            .ThenInclude(d => d.User)
                    .FirstOrDefaultAsync(p => p.UserID == userId);

                if (patient == null)
                {
                    return NotFound("Patient not found.");
                }

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

                _cache.Set($"appointments{userId}", patient, cacheOptions);
            }

            var appointments = patient.Appointments
                .Where(a => a.Doctor != null)
                .Select(a => new
                {
                    a.AppointmentID,
                    a.AppointmentDate,
                    a.AppointmentTime,
                    status = a.Status.ToString(),
                    a.Note,
                    Doctor = new
                    {
                        FirstName = a.Doctor.User?.FirstName,
                        LastName = a.Doctor.User?.LastName,
                    }
                })
                .ToList();

            return Ok(appointments);
        }
    }
}