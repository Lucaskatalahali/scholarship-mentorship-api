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
    
    //O próprio IdentityDbContext já configura o User

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ScholarshipApplication>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<User>()
            .ComplexProperty(u => u.Address);
    }
}