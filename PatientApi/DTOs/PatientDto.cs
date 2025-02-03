using PatientApi.Models;

namespace PatientApi.DTOs
{
    public class PatientDto
    {
        public NameDto Name { get; set; } = new();
        private string _gender = "unknown";

        public string Gender
        {
            get => _gender;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _gender = Models.Gender.Unknown.ToString().ToLower();
                }
                else if (Enum.TryParse<Gender>(value, true, out var parsedGender))
                {
                    _gender = parsedGender.ToString().ToLower();
                }
                else
                {
                    _gender = Models.Gender.Unknown.ToString().ToLower();
                }
            }
        }
        public DateTime BirthDate { get; set; }
        public bool Active { get; set; }
    }
}
