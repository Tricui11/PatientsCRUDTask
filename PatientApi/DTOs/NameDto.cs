namespace PatientApi.DTOs
{
    public class NameDto
    {
        public Guid? Id { get; set; }
        public string Use { get; set; } = "official";
        public string Family { get; set; } = null!;
        public List<string> Given { get; set; } = new();
    }
}
