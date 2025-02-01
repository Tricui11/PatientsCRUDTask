using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PatientApi.Models
{
    public class Name
    {
        [Key]
        public Guid Id { get; set; }

        public string? Use { get; set; } = "official";

        public string Family { get; set; } = null!;

        public string GivenSerialized { get; set; } = string.Empty;

        [NotMapped]
        public List<string> Given
        {
            get => string.IsNullOrEmpty(GivenSerialized) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(GivenSerialized);
            set => GivenSerialized = JsonConvert.SerializeObject(value);
        }
    }
}