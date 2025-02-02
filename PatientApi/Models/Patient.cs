using System.Text.Json.Serialization;

namespace PatientApi.Models
{
    public class Patient
    {
        public Guid NameId { get; set; }
        public Name Name { get; set; } = null!;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Gender Gender { get; set; } = Gender.Unknown;
        public DateTime BirthDate { get; set; }

        public bool Active { get; set; }
    }
}