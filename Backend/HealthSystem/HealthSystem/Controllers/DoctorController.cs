using HealthSystem.Data;
using HealthSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public DoctorController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        //Get doctor details by user ID with caching
        //[Authorize(Roles = "Doctor")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Doctor>> GetDoctor(Guid id)
        {
            var cacheKey = $"doctor:{id}";

            if (!_cache.TryGetValue(cacheKey, out Doctor doctor))
            {
                Thread.Sleep(5000);
                doctor = await _context.Doctors
                    .Include(d => d.User)
                    .Include(d => d.Appointments)
                    .FirstOrDefaultAsync(d => d.UserID == id);

                if (doctor == null)
                    return NotFound(new { message = "Doctor not found" });

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)); // 

                _cache.Set(cacheKey, doctor, cacheOptions);
            }

            return Ok(doctor);
        }

        // Get all appointments for a doctor with caching
        //[Authorize(Roles = "Doctor")]
        [HttpGet("{id}/appointments")]
        public async Task<ActionResult<List<Appointment>>> GetAppointmentsByDoctor(Guid id)
        {
            var cacheKey = $"appointments:{id}";

            if (!_cache.TryGetValue(cacheKey, out List<Appointment> appointments))
            {
                Thread.Sleep(5000);
                appointments = await _context.Appointments
                    .Where(a => a.DoctorUserID == id)
                    .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                    .Include(a => a.Doctor)
                    .ToListAsync();

                if (appointments == null || appointments.Count == 0)
                    return NotFound(new { message = "No appointments found for this doctor" });

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

                _cache.Set(cacheKey, appointments, cacheOptions);
            }

            return Ok(appointments);
        }

        //  Add or update appointment note 


        //[Authorize(Roles = "Doctor")]
        [HttpPut("appointments/{appointmentId}/notes")]
        public async Task<IActionResult> AddNoteToAppointment(int appointmentId, [FromBody] string note)
        {
            if (string.IsNullOrWhiteSpace(note))
            {
                return BadRequest(new { message = "Note cannot be empty" });
            }

            var appointment = await _context.Appointments
                                            .FirstOrDefaultAsync(a => a.AppointmentID == appointmentId);

            if (appointment == null)
            {
                return NotFound(new { message = "Appointment not found" });
            }

            appointment.Note = note.Trim();
            await _context.SaveChangesAsync();


            return Ok(new
            {
                message = "Note added successfully",
                appointment
            });
        }
    }
}