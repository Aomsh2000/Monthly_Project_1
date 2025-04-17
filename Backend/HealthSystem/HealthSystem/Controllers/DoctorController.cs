using HealthSystem.Data;
using HealthSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

		public DoctorController(AppDbContext context)
		{
			_context = context;
		}
         // Get doctor details by user ID
        [Authorize(Roles = "Doctor")]
        [HttpGet("{id}")]
		public async Task<ActionResult<Doctor>> GetDoctor(Guid id)
		{
			var doctor = await _context.Doctors
									   .Include(d => d.User) 
									   .Include(d => d.Appointments)
									   .FirstOrDefaultAsync(d => d.UserID == id);

			if (doctor == null)
			{
				return NotFound(new { message = "Doctor not found" });
			}

			return Ok(doctor);
		}

        // Get all appointments for a specific doctor by user ID
        [Authorize(Roles = "Doctor")]
        [HttpGet("{id}/appointments")]
		public async Task<ActionResult<List<Appointment>>> GetAppointmentsByDoctor(Guid id)
		{
			var appointments = await _context.Appointments
				.Where(a => a.DoctorUserID == id)
				.Include(a => a.Patient)
				.ThenInclude(p => p.User)
				.ToListAsync();

			if (appointments == null || appointments.Count == 0)
			{
				return NotFound(new { message = "No appointments found for this doctor" });
			}

			return Ok(appointments);
		}


        // Add or update note for a specific appointment
        // PUT: /api/doctor/appointments/{appointmentId}/notes
        [Authorize(Roles = "Doctor")]
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
