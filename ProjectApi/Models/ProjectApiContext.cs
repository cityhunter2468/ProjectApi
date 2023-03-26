using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ProjectApi.Models
{
    public partial class ProjectApiContext : DbContext
    {
        public ProjectApiContext()
        {
        }

        public ProjectApiContext(DbContextOptions<ProjectApiContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; } = null!;
        public virtual DbSet<AccountCourse> AccountCourses { get; set; } = null!;
        public virtual DbSet<Course> Courses { get; set; } = null!;
        public virtual DbSet<File> Files { get; set; } = null!;
        public virtual DbSet<Folder> Folders { get; set; } = null!;
        public virtual DbSet<Question> Questions { get; set; } = null!;
        public virtual DbSet<Test> Tests { get; set; } = null!;
        public virtual DbSet<TestRecord> TestRecords { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("server=ADMIN\\SQLEXPRESS; database=ProjectApi; uid=JDBC; password=123456789");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DisplayName)
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.ExpiredAt).HasColumnType("datetime");

                entity.Property(e => e.Password).HasMaxLength(30);

                entity.Property(e => e.RefreshToken).HasMaxLength(50);

                entity.Property(e => e.Username).HasMaxLength(30);
            });

            modelBuilder.Entity<AccountCourse>(entity =>
            {
                entity.ToTable("Account_Course");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.AccountCourses)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Account_Course_Accounts");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.AccountCourses)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Account_Course_Courses");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.Property(e => e.CourseDescription).HasColumnType("text");

                entity.Property(e => e.CourseName).HasMaxLength(50);

                entity.Property(e => e.CreateAt).HasColumnType("datetime");

                //entity.HasOne(d => d.Account)
                //    .WithMany(p => p.Courses)
                //    .HasForeignKey(d => d.AccountId)
                //    .OnDelete(DeleteBehavior.ClientSetNull)
                //    .HasConstraintName("FK_Courses_Accounts");
            });

            modelBuilder.Entity<File>(entity =>
            {
                entity.Property(e => e.CreateAt).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(200);

                entity.Property(e => e.Url).IsUnicode(false);

                entity.HasOne(d => d.Folder)
                    .WithMany(p => p.Files)
                    .HasForeignKey(d => d.FolderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Files_Folders");
            });

            modelBuilder.Entity<Folder>(entity =>
            {
                entity.Property(e => e.CreateAt).HasColumnType("datetime");

                entity.Property(e => e.FolderName).HasMaxLength(200);

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Folders)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Folders_Courses");
            });

            modelBuilder.Entity<Question>(entity =>
            {
                entity.Property(e => e.Ans).HasColumnType("text");

                entity.Property(e => e.CreateAt).HasColumnType("datetime");

                entity.Property(e => e.Ques).HasColumnType("text");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Questions)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Questions_Accounts");

                entity.HasOne(d => d.Test)
                    .WithMany(p => p.Questions)
                    .HasForeignKey(d => d.TestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Questions_Test");
            });

            modelBuilder.Entity<Test>(entity =>
            {
                entity.ToTable("Test");

                entity.Property(e => e.CreateAt).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(500);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Tests)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Test_Accounts");
            });

            modelBuilder.Entity<TestRecord>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("TestRecord");

                entity.Property(e => e.AnsRecord).HasColumnType("text");

                entity.Property(e => e.CreateAt).HasColumnType("datetime");

                entity.Property(e => e.QuesRecord).HasColumnType("text");

                entity.Property(e => e.TestRecord1)
                    .HasColumnType("text")
                    .HasColumnName("TestRecord");

                entity.HasOne(d => d.Account)
                    .WithMany()
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TestRecord_Accounts");

                entity.HasOne(d => d.Test)
                    .WithMany()
                    .HasForeignKey(d => d.TestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TestRecord_Test");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
