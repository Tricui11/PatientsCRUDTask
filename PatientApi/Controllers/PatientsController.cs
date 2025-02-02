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
                    Name = new NameDto()
                    {
                        Id = p.Name.Id,
                        Family = p.Name.Family,
                        Given = p.Name.Given,
                        Use = p.Name.Use,
                    },
                    Gender = p.Gender.ToString(),
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
                Name = new NameDto()
                {
                    Id = patient.Name.Id,
                    Family = patient.Name.Family,
                    Given = patient.Name.Given,
                    Use = patient.Name.Use
                },
                Gender = patient.Gender.ToString(),
                BirthDate = patient.BirthDate,
                Active = patient.Active,
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
                    Id = dto.Name.Id ?? Guid.NewGuid(),
                    Use = dto.Name.Use,
                    Family = dto.Name.Family,
                    Given = dto.Name.Given
                }
            };
            patient.NameId = patient.Name.Id;

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPatient), new { id = patient.Name.Id }, new PatientDto
            {
                Name = new NameDto()
                {
                    Id = patient.Name.Id,
                    Family = patient.Name.Family,
                    Given = patient.Name.Given,
                    Use = patient.Name.Use
                },
                Gender = patient.Gender.ToString(),
                BirthDate = patient.BirthDate,
                Active = patient.Active,
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

            patient.Name.Family = dto.Name.Family;
            patient.Name.Given = dto.Name.Given;
            patient.Name.Use = dto.Name.Use;
            patient.Gender = Enum.TryParse(dto.Gender, true, out Gender gender) ? gender : Gender.Unknown;
            patient.BirthDate = dto.BirthDate;
            patient.Active = dto.Active;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(Guid id)
        {
            var patient = await _context.Patients.Include(x => x.Name).FirstOrDefaultAsync(x => x.NameId == id);
            if (patient == null)
                return NotFound();

            _context.Patients.Remove(patient);
            _context.Names.Remove(patient.Name);

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
                    Name = new NameDto()
                    {
                        Id = p.Name.Id,
                        Family = p.Name.Family,
                        Use = p.Name.Use,
                        Given = p.Name.Given
                    },
                    Gender = p.Gender.ToString(),
                    BirthDate = p.BirthDate,
                    Active = p.Active
                })
                .ToListAsync();

            return Ok(patients);
        }
    }
}