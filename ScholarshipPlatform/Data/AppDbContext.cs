using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ScholarshipPlatform.Courses;
using ScholarshipPlatform.Feedbacks;
using ScholarshipPlatform.Notifications;
using ScholarshipPlatform.Payments;
using ScholarshipPlatform.ScholarshipApplications;
using ScholarshipPlatform.Scholarships;
using ScholarshipPlatform.Users;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ScholarshipPlatform.SuccessStories;
using System.Text.Json;

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
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<SuccessStory> SuccessStories => Set<SuccessStory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 1. Configurações da Bolsa (Scholarship)
        builder.Entity<Scholarship>(entity =>
        {
        entity.Property(s => s.ScholarshipType)
            .HasConversion<string>()
            .HasMaxLength(50);

            var levelsConverter = new ValueConverter<List<ScholarshipLevel>, string>(
                v => JsonSerializer.Serialize(
                    v.Select(e => e.ToString()).ToList(),
                    (JsonSerializerOptions?)null
                ),
                v => (JsonSerializer.Deserialize<List<string>>(
                        v, (JsonSerializerOptions?)null) ?? new())
                    .Select(e => Enum.Parse<ScholarshipLevel>(e))
                    .ToList()
            );

            var levelsComparer = new ValueComparer<List<ScholarshipLevel>>(
                (a, b) => a != null && b != null && a.SequenceEqual(b),
                v => v.Aggregate(0, (hash, item) =>
                    HashCode.Combine(hash, item.GetHashCode())),
                v => v.ToList()
            );

            entity.Property(s => s.Levels)
                .HasConversion(levelsConverter)
                .Metadata.SetValueComparer(levelsComparer);
        });

        // Relação Many-to-Many entre bolsas e cursos
        builder.Entity<Scholarship>()
            .HasMany(s => s.Courses)
            .WithMany()
            .UsingEntity(j => j.ToTable("ScholarshipCourses"));

        // 2. Configurações da Candidatura (ScholarshipApplication)
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

            // Mapeamentos de enums simples como string
            entity.Property(a => a.Status)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(a => a.SelectedLevel)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Tabelas de junção Many-to-Many distintas para Course
            entity.HasMany(a => a.SelectedCourses)
                .WithMany()
                .UsingEntity(j => j.ToTable("ApplicationSelectedCourses"));

            entity.HasMany(a => a.EnrolledCourses)
                .WithMany()
                .UsingEntity(j => j.ToTable("ApplicationEnrolledCourses"));
        });

        // 3. Configurações do Usuário (User)
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

        // 4. Configurações dos Documentos (UserDocument)
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

        // 5. Configurações de Notificações (Notification)
        builder.Entity<Notification>(entity =>
        {
            entity.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(n => n.Title).HasMaxLength(150);
            entity.Property(n => n.Message).HasMaxLength(1000);

            entity.HasIndex(n => new { n.UserId, n.IsRead });
        });

        // 6. Configurações de Newsletter (NewsletterSubscription)
        builder.Entity<NewsletterSubscription>(entity =>
        {
            entity.Property(n => n.Email).HasMaxLength(256).IsRequired();
            entity.Property(n => n.UnsubscribeToken).HasMaxLength(64).IsRequired();

            entity.HasIndex(n => n.Email).IsUnique();
            entity.HasIndex(n => n.UnsubscribeToken).IsUnique();
        });

        // 7. Configurações de Feedback
        builder.Entity<Feedback>(entity =>
        {
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(f => f.Type)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(f => f.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(f => f.Message)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(f => f.ContactEmail)
                .HasMaxLength(150);

            entity.Property(f => f.ContactName)
                .HasMaxLength(100);

            entity.Property(f => f.PageUrl)
                .HasMaxLength(300);

            entity.HasIndex(f => new { f.Status, f.Type });
        });

        builder.Entity<SuccessStory>(entity =>
        {
            entity.Property(s => s.MenteeName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(s => s.ImageUrl)
                .HasMaxLength(300)
                .IsRequired();

            entity.Property(s => s.ScholarshipName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(s => s.AcademicLevel)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(s => s.Country)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(s => s.Institution)
                .HasMaxLength(150);

            entity.Property(s => s.Quote)
                .HasMaxLength(500);

            entity.Property(s => s.Testimonial)
                .HasMaxLength(2000);

            entity.Property(s => s.VideoUrl)
                .HasMaxLength(300);
        });
    }
}