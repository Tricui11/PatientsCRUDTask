namespace PatientApi.Models
{
    public class Patient
    {
        public Guid NameId { get; set; }
        public Name Name { get; set; } = null!;

        public Gender Gender { get; set; }

        public DateTime BirthDate { get; set; }

        public bool Active { get; set; }
    }
}