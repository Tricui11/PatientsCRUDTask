using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientApi.Data;
using PatientApi.DTOs;
using PatientApi.Models;

namespace PatientApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PatientDto>>> GetPatients()
        {
            var patients = await _context.Patients
                .Include(p => p.Name)
                .Select(p => new PatientDto
                {
                    Id = p.Name.Id,
                    Family = p.Name.Family,
                    Given = p.Name.Given,
                    Gender = p.Gender.ToString(),
                    Use = p.Name.Use,
                    BirthDate = p.BirthDate,
                    Active = p.Active
                })
                .ToListAsync();

            return Ok(patients);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PatientDto>> GetPatient(Guid id)
        {
            var patient = await _context.Patients
                .Include(p => p.Name)
                .FirstOrDefaultAsync(p => p.Name.Id == id);

            if (patient == null)
                return NotFound();

            var patientDto = new PatientDto
            {
                Id = patient.Name.Id,
                Family = patient.Name.Family,
                Given = patient.Name.Given,
                Gender = patient.Gender.ToString(),
                BirthDate = patient.BirthDate,
                Active = patient.Active,
                Use = patient.Name.Use
            };

            return Ok(patientDto);
        }


        [HttpPost]
        public async Task<ActionResult<PatientDto>> CreatePatient(PatientDto dto)
        {
            var patient = new Patient
            {
                Gender = Enum.TryParse(dto.Gender, true, out Gender gender) ? gender : Gender.Unknown,
                BirthDate = dto.BirthDate,
                Active = dto.Active,
                Name = new Name
                {
                    Id = Guid.NewGuid(),
                    Use = dto.Use,
                    Family = dto.Family,
                    Given = dto.Given
                }
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPatient), new { id = patient.Name.Id }, new PatientDto
            {
                Id = patient.Name.Id,
                Family = patient.Name.Family,
                Given = patient.Name.Given,
                Gender = patient.Gender.ToString(),
                BirthDate = patient.BirthDate,
                Active = patient.Active,
                Use = dto.Use
            });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(Guid id, PatientDto dto)
        {
            var patient = await _context.Patients
                .Include(p => p.Name)
                .FirstOrDefaultAsync(p => p.Name.Id == id);

            if (patient == null)
                return NotFound();

            patient.Name.Family = dto.Family;
            patient.Name.Given = dto.Given;
            patient.Name.Use = dto.Use;
            patient.Gender = Enum.TryParse(dto.Gender, true, out Gender gender) ? gender : Gender.Unknown;
            patient.BirthDate = dto.BirthDate;
            patient.Active = dto.Active;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(Guid id)
        {
            var patient = await _context.Patients.Include(x => x.Name).FirstOrDefaultAsync(x => x.Name.Id == id);
            if (patient == null)
                return NotFound();

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<PatientDto>>> SearchPatients([FromQuery] string birthDate)
        {
            if (string.IsNullOrEmpty(birthDate))
                return BadRequest("birthDate parameter is required.");

            var query = _context.Patients.Include(x => x.Name).AsQueryable();

            if (birthDate.StartsWith("gt"))
            {
                if (DateTime.TryParse(birthDate.Substring(2), out var date))
                    query = query.Where(p => p.BirthDate > date);
                else
                    return BadRequest("Invalid birthDate format.");
            }
            else if (birthDate.StartsWith("lt"))
            {
                if (DateTime.TryParse(birthDate.Substring(2), out var date))
                    query = query.Where(p => p.BirthDate < date);
                else
                    return BadRequest("Invalid birthDate format.");
            }
            else if (birthDate.StartsWith("ge"))
            {
                if (DateTime.TryParse(birthDate.Substring(2), out var date))
                    query = query.Where(p => p.BirthDate >= date);
                else
                    return BadRequest("Invalid birthDate format.");
            }
            else if (birthDate.StartsWith("le"))
            {
                if (DateTime.TryParse(birthDate.Substring(2), out var date))
                    query = query.Where(p => p.BirthDate <= date);
                else
                    return BadRequest("Invalid birthDate format.");
            }
            else if (DateTime.TryParse(birthDate, out var exactDate))
            {
                query = query.Where(p => p.BirthDate.Date == exactDate.Date);
            }
            else
            {
                return BadRequest("Invalid birthDate format.");
            }

            var patients = await query
                .Include(p => p.Name)
                .Select(p => new PatientDto
                {
                    Id = p.Name.Id,
                    Family = p.Name.Family,
                    Given = p.Name.Given,
                    Gender = p.Gender.ToString(),
                    BirthDate = p.BirthDate,
                    Use = p.Name.Use,
                    Active = p.Active
                })
                .ToListAsync();

            return Ok(patients);
        }
    }
}