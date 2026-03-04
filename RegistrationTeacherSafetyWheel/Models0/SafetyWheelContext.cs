using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.IO;
namespace RegistrationCuratorSafetyWheel.Models;

public partial class CozyTestContext : DbContext
{
    public CozyTestContext()
    {
    }

    public CozyTestContext(DbContextOptions<CozyTestContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attempt> Attempts { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<QuestionType> QuestionTypes { get; set; }

    public virtual DbSet<Participant> Participants { get; set; }

    public virtual DbSet<ParticipantAnswer> ParticipantAnswers { get; set; }

    public virtual DbSet<Topic> Topics { get; set; }

    public virtual DbSet<Curator> Curators { get; set; }

    public virtual DbSet<Test> Tests { get; set; }

    public virtual DbSet<DTestType> DTestTypes { get; set; }

    string connStr = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "connection.settings"));
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer(connStr);

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
            entity.Property(e => e.ParticipantId).HasColumnName("Participants_ID");
            entity.Property(e => e.TestId).HasColumnName("Test_ID");

            entity.HasOne(d => d.Participants).WithMany(p => p.Attempts)
                .HasForeignKey(d => d.ParticipantId)
                .HasConstraintName("FK_Attempts_Participants1");

            entity.HasOne(d => d.DTestTypeNavigation).WithMany(p => p.Attempts)
                .HasForeignKey(d => d.DTestType)
                .HasConstraintName("FK_Attempts_DTestType");

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

        modelBuilder.Entity<Participant>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CuratorCreateId).HasColumnName("Curators_ID");

            entity.HasOne(d => d.Curators).WithMany(p => p.Participants)
                .HasForeignKey(d => d.CuratorCreateId)
                .HasConstraintName("FK_Participants_Curators");
        });

        modelBuilder.Entity<ParticipantAnswer>(entity =>
        {
            entity.HasKey(e => new { e.AttemptId, e.QuestionId, e.OptionId })
                  .HasName("PK_ParticipantAnswers");

            entity.Property(e => e.AttemptId).HasColumnName("Attempt_ID");
            entity.Property(e => e.QuestionId).HasColumnName("Question_ID");
            entity.Property(e => e.OptionId).HasColumnName("Option_ID");
            entity.Property(e => e.AnsweredAt)
                .HasColumnType("datetime")
                .HasColumnName("Answered_At");
            entity.Property(e => e.IsCorrect).HasColumnName("IsCorrect");

            entity.HasOne(d => d.Attempt)
                  .WithMany(p => p.ParticipantAnswers)
                  .HasForeignKey(d => d.AttemptId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_ParticipantAnswers_Attempts1");

            entity.HasOne(d => d.Question)
                  .WithMany(p => p.ParticipantAnswers)
                  .HasForeignKey(d => d.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_ParticipantAnswers_Questions1");

            entity.HasOne(d => d.Option)
                  .WithMany(p => p.ParticipantAnswers)
                  .HasForeignKey(d => d.OptionId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_ParticipantAnswers_Options1");
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.ToTable("Topic");

            entity.Property(e => e.Id).HasColumnName("ID");
        });

        modelBuilder.Entity<Curator>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.PenaltyMax).HasColumnName("Penalty_Max");
            entity.Property(e => e.TopicId).HasColumnName("Topic_ID");
            entity.Property(e => e.CuratorId).HasColumnName("Curator_ID");

            entity.HasOne(d => d.Topic).WithMany(p => p.Tests)
                .HasForeignKey(d => d.TopicId)
                .HasConstraintName("FK_Tests_Topic");

            entity.HasOne(d => d.Curator).WithMany(p => p.Tests)
                .HasForeignKey(d => d.CuratorId)
                .HasConstraintName("FK_Tests_Curators1");
        });

        modelBuilder.Entity<DTestType>(entity =>
        {
            entity.ToTable("DTestType");

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
