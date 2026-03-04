using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.IO;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
namespace Safety_Wheel.Models;

public partial class SafetyWheelContext : DbContext
{
    public SafetyWheelContext()
    {
    }

    public SafetyWheelContext(DbContextOptions<SafetyWheelContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attempt> Attempts { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<QuestionType> QuestionTypes { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentAnswer> StudentAnswers { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<Test> Tests { get; set; }

    public virtual DbSet<TestType> TestTypes { get; set; }


    string connStr = File.ReadAllText(Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "connection.settings"));


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(connStr);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attempt>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.FinishedAt)
                .HasColumnType("datetime")
                .HasColumnName("Finished_At");
            entity.Property(e => e.StartedAt)
                .HasColumnType("datetime")
                .HasColumnName("Started_At");
            entity.Property(e => e.StudentsId).HasColumnName("Students_ID");
            entity.Property(e => e.TestId).HasColumnName("Test_ID");

            entity.HasOne(d => d.Students).WithMany(p => p.Attempts)
                .HasForeignKey(d => d.StudentsId)
                .HasConstraintName("FK_Attempts_Students1");

            entity.HasOne(d => d.TestTypeNavigation).WithMany(p => p.Attempts)
                .HasForeignKey(d => d.TestType)
                .HasConstraintName("FK_Attempts_TestType");

            entity.HasOne(d => d.Test)
                .WithMany(p => p.Attempts)
                .HasForeignKey(d => d.TestId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Attempts_Tests");

        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.QuestionId).HasColumnName("Question_ID");
            entity.Property(e => e.TextAnswer).HasColumnName("Text_Answer");

            entity.HasOne(d => d.Question)
                .WithMany(p => p.Options)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Options_Questions1");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.PicturePath).HasColumnName("Picture_Path");
            entity.Property(e => e.TestId).HasColumnName("Test_ID");
            entity.Property(e => e.TestQuest).HasColumnName("Test_Quest");

            entity.HasOne(d => d.QuestionTypeNavigation).WithMany(p => p.Questions)
                .HasForeignKey(d => d.QuestionType)
                .HasConstraintName("FK_Questions_QuestionType");

            entity.HasOne(d => d.Test)
                .WithMany(p => p.Questions)
                .HasForeignKey(d => d.TestId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Questions_Tests");
        });

        modelBuilder.Entity<QuestionType>(entity =>
        {
            entity.ToTable("QuestionType");

            entity.Property(e => e.Id).HasColumnName("ID");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.TeachersId).HasColumnName("Teachers_ID");

            entity.HasOne(d => d.Teachers).WithMany(p => p.Students)
                .HasForeignKey(d => d.TeachersId)
                .HasConstraintName("FK_Students_Teachers");
        });

        modelBuilder.Entity<StudentAnswer>(entity =>
        {
            entity.HasKey(e => new { e.AttemptId, e.QuestionId, e.OptionId })
                  .HasName("PK_StudentAnswers");

            entity.Property(e => e.AttemptId).HasColumnName("Attempt_ID");
            entity.Property(e => e.QuestionId).HasColumnName("Question_ID");
            entity.Property(e => e.OptionId).HasColumnName("Option_ID");
            entity.Property(e => e.AnsweredAt)
                .HasColumnType("datetime")
                .HasColumnName("Answered_At");
            entity.Property(e => e.IsCorrect).HasColumnName("IsCorrect");

            entity.HasOne(d => d.Attempt)
                  .WithMany(p => p.StudentAnswers)
                  .HasForeignKey(d => d.AttemptId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_StudentAnswers_Attempts1");

            entity.HasOne(d => d.Question)
                  .WithMany(p => p.StudentAnswers)
                  .HasForeignKey(d => d.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_StudentAnswers_Questions1");

            entity.HasOne(d => d.Option)
                  .WithMany(p => p.StudentAnswers)
                  .HasForeignKey(d => d.OptionId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_StudentAnswers_Options1");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("Subject");

            entity.Property(e => e.Id).HasColumnName("ID");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.PenaltyMax).HasColumnName("Penalty_Max");
            entity.Property(e => e.SubjectId).HasColumnName("Subject_ID");
            entity.Property(e => e.TeacherId).HasColumnName("Teacher_ID");

            entity.HasOne(d => d.Subject).WithMany(p => p.Tests)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("FK_Tests_Subject");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Tests)
                .HasForeignKey(d => d.TeacherId)
                .HasConstraintName("FK_Tests_Teachers1");
        });

        modelBuilder.Entity<TestType>(entity =>
        {
            entity.ToTable("TestType");

            entity.Property(e => e.Id)
                  .ValueGeneratedOnAdd()
                  .HasColumnType("int")
                  .HasColumnName("ID");

            entity.Property(e => e.TimeLimitSecond)
                    .HasColumnType("int")
                    .HasDefaultValue(1200);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
