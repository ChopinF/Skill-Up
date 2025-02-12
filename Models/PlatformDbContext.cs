using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using platform.Models.Account;
using platform.Models.Coaches;
using platform.Models.Courses;

namespace platform.Models
{
    public class PlatformDbContext : IdentityDbContext<User>
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<PendingCourse> PendingCourses { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Coach> Coaches { get; set; }
        public DbSet<PendingCoach> PendingCoaches { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);

                entity.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(u => u.LastName).IsRequired().HasMaxLength(50);
                entity.Property(u => u.ActivationCode).IsRequired();
                entity.Property(u => u.ResetPasswordCode).HasMaxLength(100);

                entity
                    .HasMany(u => u.Courses)
                    .WithOne(c => c.User)
                    .HasForeignKey(c => c.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity
                    .HasMany(u => u.PendingCourses)
                    .WithOne(pc => pc.User)
                    .HasForeignKey(pc => pc.PendingCourseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity
                    .HasMany(u => u.Purchases)
                    .WithOne(p => p.User)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity
                    .HasOne(u => u.Coach)
                    .WithOne(c => c.User)
                    .HasForeignKey<Coach>(c => c.CoachId)
                    .IsRequired(false);

                entity
                    .HasOne(u => u.PendingCoach)
                    .WithOne(c => c.User)
                    .HasForeignKey<PendingCoach>(c => c.PendingCoachId)
                    .IsRequired(false);

                entity
                    .HasMany(u => u.Bookings)
                    .WithOne(b => b.User)
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Courses");
                entity.HasKey(c => c.CourseId);

                entity.Property(c => c.Price).IsRequired();
                entity.Property(c => c.Title).IsRequired();
                entity.Property(c => c.Description).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Path).IsRequired();
                entity.Property(c => c.Genre).IsRequired();

                entity.HasOne(c => c.User).WithMany(u => u.Courses).HasForeignKey(c => c.UserId);
            });

            modelBuilder.Entity<PendingCourse>(entity =>
            {
                entity.ToTable("PendingCourses");
                entity.HasKey(pc => pc.PendingCourseId);

                entity.Property(pc => pc.Price).IsRequired();
                entity.Property(pc => pc.Description).IsRequired().HasMaxLength(100);
                entity.Property(pc => pc.Path).IsRequired();
                entity.Property(pc => pc.Title).IsRequired();
                entity.Property(pc => pc.Title).IsRequired();

                entity
                    .HasOne(pc => pc.User)
                    .WithMany(u => u.PendingCourses)
                    .HasForeignKey(pc => pc.UserId);
            });

            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.ToTable("Purchases");
                entity.HasKey(p => new { p.UserId, p.CourseId });

                entity.Property(p => p.AmountPaid).IsRequired();
                entity.Property(p => p.PurchaseDate).IsRequired();

                entity
                    .HasOne(p => p.User)
                    .WithMany(u => u.Purchases)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity
                    .HasOne(p => p.Course)
                    .WithMany(c => c.Purchases)
                    .HasForeignKey(p => p.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Coach>(entity =>
            {
                entity.ToTable("Coaches");
                entity.HasKey(c => c.CoachId);

                entity.Property(c => c.PhoneNumber).IsRequired();
                entity.Property(c => c.PicturePath).IsRequired();
                entity.Property(c => c.Bio).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Level).IsRequired();
                entity.Property(c => c.City).IsRequired();
                entity.Property(c => c.ExpertiseArea).IsRequired();

                entity.HasIndex(p => p.UserId).IsUnique();

                entity
                    .HasOne(c => c.User)
                    .WithOne(u => u.Coach)
                    .HasForeignKey<Coach>(c => c.UserId)
                    .IsRequired();

                entity
                    .HasMany(c => c.Bookings)
                    .WithOne(b => b.Coach)
                    .HasForeignKey(b => b.CoachId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PendingCoach>(entity =>
            {
                entity.ToTable("PendingCoaches");
                entity.HasKey(c => c.PendingCoachId);

                entity.Property(c => c.PhoneNumber).IsRequired();
                entity.Property(c => c.PicturePath).IsRequired();
                entity.Property(c => c.Bio).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Level).IsRequired();
                entity.Property(c => c.City).IsRequired();
                entity.Property(c => c.ExpertiseArea).IsRequired();

                entity.HasIndex(p => p.UserId).IsUnique();

                entity
                    .HasOne(c => c.User)
                    .WithOne(u => u.PendingCoach)
                    .HasForeignKey<PendingCoach>(c => c.UserId)
                    .IsRequired();
            });

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.ToTable("Bookings");
                entity.HasKey(b => new { b.UserId, b.CoachId });

                entity.Property(b => b.UserId).IsRequired();
                entity.Property(b => b.CoachId).IsRequired();
                entity.Property(b => b.Date).IsRequired();
                entity.Property(b => b.IsConfirmed).IsRequired();

                entity
                    .HasOne(b => b.User)
                    .WithMany(u => u.Bookings)
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity
                    .HasOne(b => b.Coach)
                    .WithMany(c => c.Bookings)
                    .HasForeignKey(b => b.CoachId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public PlatformDbContext(DbContextOptions<PlatformDbContext> options)
            : base(options) { }
    }
}
