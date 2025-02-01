using Microsoft.EntityFrameworkCore;
using PatientApi.Models;

namespace PatientApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Name> Names { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.Name)
                .WithOne()
                .HasForeignKey<Patient>(p => p.NameId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Patient>()
                .HasKey(p => p.NameId);

            modelBuilder.Entity<Patient>()
                .HasIndex(p => p.BirthDate);

            modelBuilder.Entity<Patient>()
                .Property(p => p.Gender)
                .HasConversion<string>();

            modelBuilder.Entity<Patient>()
                .Property(n => n.BirthDate)
                .IsRequired();

            modelBuilder.Entity<Name>()
                .HasKey(n => n.Id);

            modelBuilder.Entity<Name>()
                .Property(n => n.Family)
                .IsRequired();
        }
    }
}
