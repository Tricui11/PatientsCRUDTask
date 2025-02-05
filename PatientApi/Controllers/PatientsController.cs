using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientApi.Data;
using PatientApi.DTOs;
using PatientApi.Helpers;
using PatientApi.Models;
using System.Linq.Expressions;

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

        /// <summary>
        /// Получить список всех пациентов.
        /// </summary>
        /// <returns>Список пациентов.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PatientDto>>> GetPatients()
        {
            var patients = await _context.Patients
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

        /// <summary>
        /// Получить информацию о пациенте по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор пациента.</param>
        /// <returns>Объект пациента.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<PatientDto>> GetPatient(Guid id)
        {
            var patient = await _context.Patients
                .Include(p => p.Name)
                .FirstOrDefaultAsync(p => p.Name.Id == id);

            if (patient == null)
            {
                return NotFound();
            }

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

        /// <summary>
        /// Создать нового пациента.
        /// </summary>
        /// <param name="dto">Данные пациента.</param>
        /// <returns>Созданный пациент.</returns>
        [HttpPost]
        public async Task<ActionResult<PatientDto>> CreatePatient(PatientDto dto)
        {
            var birthDateWithoutMilliseconds = dto.BirthDate.AddMilliseconds(-dto.BirthDate.Millisecond);

            var patient = new Patient
            {
                Gender = Enum.TryParse(dto.Gender, true, out Gender gender) ? gender : Gender.Unknown,
                BirthDate = birthDateWithoutMilliseconds,
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

        /// <summary>
        /// Обновить информацию о пациенте.
        /// </summary>
        /// <param name="id">Идентификатор пациента.</param>
        /// <param name="dto">Новые данные пациента.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(Guid id, PatientDto dto)
        {
            var patient = await _context.Patients
                .Include(p => p.Name)
                .FirstOrDefaultAsync(p => p.Name.Id == id);

            if (patient == null)
            {
                return NotFound();
            }

            patient.Name.Family = dto.Name.Family;
            patient.Name.Given = dto.Name.Given;
            patient.Name.Use = dto.Name.Use;
            patient.Gender = Enum.TryParse(dto.Gender, true, out Gender gender) ? gender : Gender.Unknown;
            patient.BirthDate = dto.BirthDate;
            patient.Active = dto.Active;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Удалить пациента.
        /// </summary>
        /// <param name="id">Идентификатор пациента.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(Guid id)
        {
            var name = await _context.Names.FirstOrDefaultAsync(x => x.Id == id);
            if (name == null)
            { 
                return NotFound();
            }

            _context.Names.Remove(name);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Searches for patients by birth date with optional comparison operators.
        /// </summary>
        /// <param name="birthDate">
        /// The birth date filter.  
        /// Supported formats:
        /// - Exact date: `YYYY-MM-DD` (e.g., `1985-05-20`)  
        /// - Year only: `YYYY` (e.g., `1990`)  
        /// - Year and month: `YYYY-MM` (e.g., `1990-05`)  
        /// - Comparison operators:
        ///   - `gtYYYY-MM-DD` (greater than)  
        ///   - `ltYYYY-MM-DD` (less than)  
        ///   - `geYYYY-MM-DD` (greater or equal)  
        ///   - `leYYYY-MM-DD` (less or equal)  
        ///   - `eqYYYY-MM-DD` (equal)  
        ///   - `neYYYY-MM-DD` (not equal)  
        ///   - `saYYYY-MM-DD` (start after)  
        ///   - `ebYYYY-MM-DD` (ends before)  
        ///   - `apYYYY-MM-DD` (approximately)  
        /// </param>
        /// <returns>
        /// A list of matching patients in DTO format.
        /// </returns>
        /// <response code="200">Returns the list of found patients.</response>
        /// <response code="400">If the birthDate format is invalid.</response>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<PatientDto>>> SearchPatients([FromQuery] List<string> birthDate)
        {
            if (birthDate == null || birthDate.Count == 0)
            {
                return BadRequest("At least one birthDate parameter is required.");
            }

            var query = _context.Patients.AsQueryable();

            foreach (var dateFilter in birthDate)
            {
                if (!TryParseDateFilter(dateFilter, out Expression<Func<Patient, bool>> filter))
                {
                    return BadRequest($"Invalid birthDate format: {dateFilter}");
                }

                query = query.Where(filter);
            }

            var patients = await query
                .Select(p => new PatientDto
                {
                    Name = new NameDto
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

        private static bool TryParseDateFilter(string dateFilter, out Expression<Func<Patient, bool>> filter)
        {
            filter = null;

            if (string.IsNullOrEmpty(dateFilter))
            {
                return false;
            }
            if (dateFilter.Length < 2)
            {
                return false;
            }

            string operatorPrefix = dateFilter.Substring(0, 2);
            DateTime dateTime;

            if (operatorPrefix == "gt" || operatorPrefix == "lt" || operatorPrefix == "ge" || operatorPrefix == "le" ||
                operatorPrefix == "eq" || operatorPrefix == "ne" || operatorPrefix == "sa" || operatorPrefix == "eb" || operatorPrefix == "ap")
            {
                var dateStr = dateFilter.Substring(2);

                if (!CustomDateParser.TryParseDate(dateStr, out dateTime))
                {
                    return false;
                }

                switch (operatorPrefix)
                {
                    case "gt":
                    case "sa":
                        filter = p => p.BirthDate > dateTime;
                        break;
                    case "lt":
                    case "eb":
                        filter = p => p.BirthDate < dateTime;
                        break;
                    case "ge":
                        filter = p => p.BirthDate >= dateTime;
                        break;
                    case "le":
                        filter = p => p.BirthDate <= dateTime;
                        break;
                    case "eq":
                        filter = p => p.BirthDate.Date == dateTime.Date;
                        break;
                    case "ne":
                        filter = p => p.BirthDate.Date != dateTime.Date;
                        break;
                    case "ap":
                        TimeSpan tolerance = TimeSpan.FromDays(1);
                        filter = p => p.BirthDate >= dateTime - tolerance && p.BirthDate <= dateTime + tolerance;
                        break;
                    default:
                        return false;
                }
            }
            else if (CustomDateParser.TryParseDate(dateFilter, out dateTime))
            {
                filter = p => p.BirthDate == dateTime;
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}