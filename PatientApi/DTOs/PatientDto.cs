namespace PatientApi.DTOs
{
    public class PatientDto
    {
        public Guid? Id { get; set; }
        public string Family { get; set; }
        public List<string> Given { get; set; } = new();
        public string Gender { get; set; }
        public string Use { get; set; } = "official";
        public DateTime BirthDate { get; set; }
        public bool Active { get; set; }
    }
}
