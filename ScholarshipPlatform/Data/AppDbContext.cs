using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ScholarshipPlatform.Courses;
using ScholarshipPlatform.Payments;
using ScholarshipPlatform.ScholarshipApplications;
using ScholarshipPlatform.Scholarships;
using ScholarshipPlatform.Users;

namespace ScholarshipPlatform.Data;

public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    } 

    public DbSet<Scholarship> Scholarships => Set<Scholarship>();
    public DbSet<ScholarshipApplication> ScholarshipApplications => Set<ScholarshipApplication>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Course> Courses => Set<Course>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ScholarshipApplication>(entity =>
        {
            // Relações e integridade referencial
            entity.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(a => a.Scholarship)
                .WithMany()
                .HasForeignKey(a => a.ScholarshipId)
                .OnDelete(DeleteBehavior.Restrict);

            // Mapeamentos de propriedades
            entity.Property(a => a.Status)
                .HasConversion<string>()
                .HasMaxLength(30);

            // Tabelas de junção Many-to-Many distintas para Course
            entity.HasMany(a => a.SelectedCourses)
                .WithMany()
                .UsingEntity(j => j.ToTable("ApplicationSelectedCourses"));

            entity.HasMany(a => a.EnrolledCourses)
                .WithMany()
                .UsingEntity(j => j.ToTable("ApplicationEnrolledCourses"));
        });

        builder.Entity<User>(entity =>
        {
            entity.Property(u => u.AccountStatus)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(u => u.EducationLevel)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.ComplexProperty(u => u.Address);
        });
    }
}