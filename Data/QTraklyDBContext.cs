using Demoproject.Models;
using Microsoft.EntityFrameworkCore;

namespace Demoproject.Data
{
    public class QTraklyDBContext : DbContext
    {
        public QTraklyDBContext(DbContextOptions<QTraklyDBContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<SubTask> SubTasks { get; set; }
        public DbSet<Dependency> Dependencies { get; set; }
        public DbSet<TaskStats> TaskStats { get; set; }
        public DbSet<TaskLog> TaskLogs { get; set; }
        public DbSet<DependencyRequest> DependencyRequests { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<TaskUpdateLog> TaskUpdateLogs { get; set; }
        public DbSet<DependencyFact> DependencyFacts { get; set; }
        public DbSet<TaskDependencyFact> taskDependencyFacts { get; set; }
        public DbSet<TaskDateWorkedHours> TaskDateworkedHours { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DependencyRequest>()
                .HasKey(dr => dr.DependencyTaskId);

            modelBuilder.Entity<TaskDateWorkedHours>()
                .HasKey(dr => dr.Id);
            modelBuilder.Entity<TaskDateWorkedHours>()
                .Property(dr => dr.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<TaskDependencyFact>()
                .HasKey(dr => dr.Id);

            modelBuilder.Entity<TaskDependencyFact>()
                .Property(dr => dr.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<DependencyRequest>()
                .Property(dr => dr.DependencyTaskId)
                .ValueGeneratedOnAdd();

            // DependencyFact Configuration
            modelBuilder.Entity<DependencyFact>()
                .HasKey(df => df.Id);

            modelBuilder.Entity<DependencyFact>()
                .Property(df => df.Id)
                .ValueGeneratedOnAdd();

            // One-to-One Relationship: DependencyRequest (Principal) -> DependencyFact (Dependent)
            modelBuilder.Entity<DependencyRequest>()
                .HasOne(dr => dr.DependencyFact)
                .WithOne(df => df.DependencyRequest)
                .HasForeignKey<DependencyFact>(df => df.DependencyTaskId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            // TaskItem Configuration
            modelBuilder.Entity<TaskItem>()
                .Property(t => t.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<TaskItem>()
                .Property(t => t.EstimatedHours)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TaskItem>()
                .Property(t => t.CompletedHours)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TaskItem>()
                .Property(t => t.HasSubtask)
                .HasMaxLength(3)
                .HasDefaultValue("No"); // Configure HasSubtask column

            // SubTask Configuration
            modelBuilder.Entity<SubTask>()
                .Property(s => s.Id)
                .ValueGeneratedOnAdd();

            // TaskItem → SubTasks (One-to-Many)
            modelBuilder.Entity<SubTask>()
                .HasOne(s => s.TaskItem)
                .WithMany(t => t.SubTasks)
                .HasForeignKey(s => s.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Dependency Configuration
            modelBuilder.Entity<Dependency>()
                .Property(d => d.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<DependencyRequest>()
               .Property(d => d.DependencyTaskId)
               .ValueGeneratedOnAdd();
            modelBuilder.Entity<DependencyFact>()
              .Property(d => d.Id)
              .ValueGeneratedOnAdd();

            // TaskItem → Dependencies (One-to-Many)
            modelBuilder.Entity<Dependency>()
                .HasOne(d => d.TaskItem)
                .WithMany(t => t.Dependencies)
                .HasForeignKey(d => d.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Name).HasMaxLength(255);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.Roles).IsRequired();
                entity.HasIndex(e => e.UserId).IsUnique();
            });

            // TaskLog Configuration
            modelBuilder.Entity<TaskLog>()
                .Property(tl => tl.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<TaskLog>()
                .Property(tl => tl.HoursWorked)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TaskLog>()
                .Property(tl => tl.Team)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<TaskLog>()
                .Property(tl => tl.CreatedBy)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<TaskLog>()
                .HasIndex(tl => new { tl.UserId, tl.Date, tl.TaskId, tl.SubTaskId })
                .IsUnique();

            // TaskLog → TaskItem (One-to-Many, optional)
            modelBuilder.Entity<TaskLog>()
                .HasOne(tl => tl.Task)
                .WithMany(t => t.TaskLogs)
                .HasForeignKey(tl => tl.TaskId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            // TaskLog → SubTask (One-to-Many, optional)
            modelBuilder.Entity<TaskLog>()
                .HasOne(tl => tl.SubTask)
                .WithMany(st => st.TaskLogs)
                .HasForeignKey(tl => tl.SubTaskId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            // TaskLog → User (Many-to-One)
            modelBuilder.Entity<TaskLog>()
                .HasOne(tl => tl.User)
                .WithMany(u => u.TaskLogs)
                .HasForeignKey(tl => tl.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}