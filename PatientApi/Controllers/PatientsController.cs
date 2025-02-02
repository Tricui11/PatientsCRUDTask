﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientApi.Data;
using PatientApi.DTOs;
using PatientApi.Models;
using System.Globalization;

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

        /// <summary>
        /// Создать нового пациента.
        /// </summary>
        /// <param name="dto">Данные пациента.</param>
        /// <returns>Созданный пациент.</returns>
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

        /// <summary>
        /// Удалить пациента.
        /// </summary>
        /// <param name="id">Идентификатор пациента.</param>
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
        public async Task<ActionResult<IEnumerable<PatientDto>>> SearchPatients([FromQuery] string birthDate)
        {
            if (string.IsNullOrEmpty(birthDate))
                return BadRequest("birthDate parameter is required.");

            var query = _context.Patients.AsQueryable();
            DateTimeOffset dateTimeOffset;

            string operatorPrefix = birthDate.Substring(0, 2);

            if (operatorPrefix == "gt" || operatorPrefix == "lt" || operatorPrefix == "ge" || operatorPrefix == "le" ||
                operatorPrefix == "eq" || operatorPrefix == "ne" || operatorPrefix == "sa" || operatorPrefix == "eb" || operatorPrefix == "ap")
            {
                if (!TryParseBirthDate(birthDate.Substring(2), out dateTimeOffset))
                    return BadRequest("Invalid birthDate format.");

                switch (operatorPrefix)
                {
                    case "gt":
                        query = query.Where(p => p.BirthDate > dateTimeOffset);
                        break;
                    case "lt":
                        query = query.Where(p => p.BirthDate < dateTimeOffset);
                        break;
                    case "ge":
                        query = query.Where(p => p.BirthDate >= dateTimeOffset);
                        break;
                    case "le":
                        query = query.Where(p => p.BirthDate <= dateTimeOffset);
                        break;
                    case "eq":
                        query = query.Where(p => p.BirthDate == dateTimeOffset);
                        break;
                    case "ne":
                        query = query.Where(p => p.BirthDate != dateTimeOffset);
                        break;
                    case "sa":
                        query = query.Where(p => p.BirthDate > dateTimeOffset);
                        break;
                    case "eb":
                        query = query.Where(p => p.BirthDate < dateTimeOffset);
                        break;
                    case "ap":
                        query = query.Where(p => p.BirthDate == dateTimeOffset);
                        break;
                }
            }
            else if (birthDate.Length == 4)
            {
                var year = int.Parse(birthDate);
                var startOfYear = new DateTime(year, 1, 1);
                var endOfYear = new DateTime(year, 12, 31, 23, 59, 59);
                query = query.Where(p => p.BirthDate >= startOfYear && p.BirthDate <= endOfYear);
            }
            else if (birthDate.Length == 7)
            {
                var parts = birthDate.Split('-');
                var year = int.Parse(parts[0]);
                var month = int.Parse(parts[1]);

                var startOfMonth = new DateTime(year, month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);
                query = query.Where(p => p.BirthDate >= startOfMonth && p.BirthDate <= endOfMonth);
            }
            else if (TryParseBirthDate(birthDate, out dateTimeOffset))
            {
                query = query.Where(p => p.BirthDate == dateTimeOffset);
            }
            else if (DateTime.TryParse(birthDate, out var date))
            {
                query = query.Where(p => p.BirthDate.Date == date.Date);
            }
            else
            {
                return BadRequest("Invalid birthDate format.");
            }

            var patients = await query
                .Include(p => p.Name)
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

        private static bool TryParseBirthDate(string birthDate, out DateTimeOffset dateTimeOffset)
        {
            if (DateTimeOffset.TryParseExact(birthDate, "yyyy-MM-ddTHH:mm:ss.FFFK", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out dateTimeOffset))
            {
                return true;
            }

            return DateTimeOffset.TryParse(birthDate, out dateTimeOffset);
        }
    }
}