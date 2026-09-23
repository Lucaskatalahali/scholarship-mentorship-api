using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ScholarshipPlatform.Courses;
using ScholarshipPlatform.Notifications;
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
    public DbSet<UserDocument> UserDocuments => Set<UserDocument>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NewsletterSubscription> NewsletterSubscriptions => Set<NewsletterSubscription>();
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

        builder.Entity<UserDocument>(entity =>
        {
            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(d => d.DocumentType)
                .HasConversion<string>()
                .HasMaxLength(40);

            entity.Property(d => d.OriginalFileName).HasMaxLength(255);
            entity.Property(d => d.StoredFileName).HasMaxLength(255);
            entity.Property(d => d.ContentType).HasMaxLength(100);


        });

        builder.Entity<Notification>(entity =>
        {
            entity.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(n => n.Title).HasMaxLength(150);
            entity.Property(n => n.Message).HasMaxLength(1000);

            // Índice para acelerar a busca de notificações não lidas por usuário
            entity.HasIndex(n => new { n.UserId, n.IsRead });
        });

        builder.Entity<NewsletterSubscription>(entity =>
        {
            entity.Property(n => n.Email).HasMaxLength(256).IsRequired();
            entity.Property(n => n.UnsubscribeToken).HasMaxLength(64).IsRequired();

            // Índice único no e-mail para evitar duplicidade de inscrições
            entity.HasIndex(n => n.Email).IsUnique();

            // Índice no token para busca instantânea ao clicar no link de descadastro
            entity.HasIndex(n => n.UnsubscribeToken).IsUnique();
        });
    }
}