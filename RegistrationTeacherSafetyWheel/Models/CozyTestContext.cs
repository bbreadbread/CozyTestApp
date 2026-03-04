using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RegistrationCuratorCozyTest.Models;

public partial class CozyTestContext : DbContext
{
    public CozyTestContext()
    {
    }

    public CozyTestContext(DbContextOptions<CozyTestContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<Attempt> Attempts { get; set; }

    public virtual DbSet<Curator> Curators { get; set; }

    public virtual DbSet<DQuestionType> DQuestionTypes { get; set; }

    public virtual DbSet<DTestType> DTestTypes { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<Participant> Participants { get; set; }

    public virtual DbSet<ParticipantAnswer> ParticipantAnswers { get; set; }

    public virtual DbSet<ParticipantsAssignedTest> ParticipantsAssignedTests { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Test> Tests { get; set; }

    public virtual DbSet<Topic> Topics { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=HOME-PC;Database=cozy-test;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Application>(entity =>
        {
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.DateTimeApplication).HasColumnType("datetime");
            entity.Property(e => e.ReviewerId).HasColumnName("Reviewer_ID");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.Applications)
                .HasForeignKey(d => d.ReviewerId)
                .HasConstraintName("FK_Applications_Curators");
        });

        modelBuilder.Entity<Attempt>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.FinishedAt)
                .HasColumnType("datetime")
                .HasColumnName("Finished_At");
            entity.Property(e => e.ParticipantId).HasColumnName("Participant_ID");
            entity.Property(e => e.StartedAt)
                .HasColumnType("datetime")
                .HasColumnName("Started_At");
            entity.Property(e => e.TestId).HasColumnName("Test_ID");

            entity.HasOne(d => d.Participant).WithMany(p => p.Attempts)
                .HasForeignKey(d => d.ParticipantId)
                .HasConstraintName("FK_Attempts_Participants");

            entity.HasOne(d => d.Test).WithMany(p => p.Attempts)
                .HasForeignKey(d => d.TestId)
                .HasConstraintName("FK_Attempts_Tests");
        });

        modelBuilder.Entity<Curator>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Teachers");

            entity.Property(e => e.Id).HasColumnName("ID");

            entity.HasMany(d => d.Participants).WithMany(p => p.Curators)
                .UsingEntity<Dictionary<string, object>>(
                    "CuratorsParticipant",
                    r => r.HasOne<Participant>().WithMany()
                        .HasForeignKey("ParticipantId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Curators_Participants_Participants"),
                    l => l.HasOne<Curator>().WithMany()
                        .HasForeignKey("CuratorId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Curators_Participants_Curators"),
                    j =>
                    {
                        j.HasKey("CuratorId", "ParticipantId");
                        j.ToTable("Curators_Participants");
                        j.IndexerProperty<int>("CuratorId").HasColumnName("Curator_ID");
                        j.IndexerProperty<int>("ParticipantId").HasColumnName("Participant_ID");
                    });

            entity.HasMany(d => d.Tests).WithMany(p => p.Curators)
                .UsingEntity<Dictionary<string, object>>(
                    "CuratorsTest",
                    r => r.HasOne<Test>().WithMany()
                        .HasForeignKey("TestsId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Curators_Tests_Tests"),
                    l => l.HasOne<Curator>().WithMany()
                        .HasForeignKey("CuratorsId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Curators_Tests_Curators"),
                    j =>
                    {
                        j.HasKey("CuratorsId", "TestsId");
                        j.ToTable("Curators_Tests");
                        j.IndexerProperty<int>("CuratorsId").HasColumnName("Curators_ID");
                        j.IndexerProperty<int>("TestsId").HasColumnName("Tests_ID");
                    });
        });

        modelBuilder.Entity<DQuestionType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_QuestionType");

            entity.ToTable("D_QuestionTypes", tb => tb.HasComment("Картинка + один ответ\r\nМного картинок + много ответов\r\nКартинка + много ответов"));

            entity.Property(e => e.Id).HasColumnName("ID");
        });

        modelBuilder.Entity<DTestType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TestType");

            entity.ToTable("D_TestTypes", tb => tb.HasComment("Тест, опросник"));

            entity.Property(e => e.Id).HasColumnName("ID");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CuratorId).HasColumnName("Curator_ID");

            entity.HasOne(d => d.Curator).WithMany(p => p.Groups)
                .HasForeignKey(d => d.CuratorId)
                .HasConstraintName("FK_Groups_Curators");

            entity.HasMany(d => d.Participants).WithMany(p => p.Groups)
                .UsingEntity<Dictionary<string, object>>(
                    "GroupsParticipant",
                    r => r.HasOne<Participant>().WithMany()
                        .HasForeignKey("ParticipantId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Groups_Participants_Participants"),
                    l => l.HasOne<Group>().WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Groups_Participants_Groups"),
                    j =>
                    {
                        j.HasKey("GroupId", "ParticipantId");
                        j.ToTable("Groups_Participants");
                        j.IndexerProperty<int>("GroupId").HasColumnName("Group_ID");
                        j.IndexerProperty<int>("ParticipantId").HasColumnName("Participant_ID");
                    });
        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IsCorrect).HasDefaultValue(false);
            entity.Property(e => e.QuestionId).HasColumnName("Question_ID");
            entity.Property(e => e.TextAnswer).HasColumnName("Text_Answer");

            entity.HasOne(d => d.Question).WithMany(p => p.Options)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK_Options_Questions");
        });

        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Students");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CuratorCreateId).HasColumnName("Curator_Create_ID");

            entity.HasOne(d => d.CuratorCreate).WithMany(p => p.ParticipantsNavigation)
                .HasForeignKey(d => d.CuratorCreateId)
                .HasConstraintName("FK_Participants_Curators1");

            entity.HasMany(d => d.Tests).WithMany(p => p.Participants)
                .UsingEntity<Dictionary<string, object>>(
                    "ParticipantsFavoriteTest",
                    r => r.HasOne<Test>().WithMany()
                        .HasForeignKey("TestId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Participants_Favorite_Tests_Tests"),
                    l => l.HasOne<Participant>().WithMany()
                        .HasForeignKey("ParticipantId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Participants_Favorite_Tests_Participants"),
                    j =>
                    {
                        j.HasKey("ParticipantId", "TestId");
                        j.ToTable("Participants_Favorite_Tests");
                        j.IndexerProperty<int>("ParticipantId").HasColumnName("Participant_ID");
                        j.IndexerProperty<int>("TestId").HasColumnName("Test_ID");
                    });
        });

        modelBuilder.Entity<ParticipantAnswer>(entity =>
        {
            entity.HasKey(e => new { e.AttemptId, e.QuestionId, e.OptionId });

            entity.Property(e => e.AttemptId).HasColumnName("Attempt_ID");
            entity.Property(e => e.QuestionId).HasColumnName("Question_ID");
            entity.Property(e => e.OptionId).HasColumnName("Option_ID");
            entity.Property(e => e.AnsweredAt)
                .HasColumnType("datetime")
                .HasColumnName("Answered_At");

            entity.HasOne(d => d.Attempt).WithMany(p => p.ParticipantAnswers)
                .HasForeignKey(d => d.AttemptId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ParticipantAnswers_Attempts");

            entity.HasOne(d => d.Option).WithMany(p => p.ParticipantAnswers)
                .HasForeignKey(d => d.OptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ParticipantAnswers_Options");

            entity.HasOne(d => d.Question).WithMany(p => p.ParticipantAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ParticipantAnswers_Questions");
        });

        modelBuilder.Entity<ParticipantsAssignedTest>(entity =>
        {
            entity.HasKey(e => new { e.ParticipantId, e.TestId });

            entity.ToTable("Participants_Assigned_Tests");

            entity.Property(e => e.ParticipantId).HasColumnName("Participant_ID");
            entity.Property(e => e.TestId).HasColumnName("Test_ID");
            entity.Property(e => e.DateTimeAssigned).HasColumnType("datetime");

            entity.HasOne(d => d.Participant).WithMany(p => p.ParticipantsAssignedTests)
                .HasForeignKey(d => d.ParticipantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Participants_Assigned_Tests_Participants");

            entity.HasOne(d => d.Test).WithMany(p => p.ParticipantsAssignedTests)
                .HasForeignKey(d => d.TestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Participants_Assigned_Tests_Tests");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.PicturePath).HasColumnName("Picture_Path");
            entity.Property(e => e.QuestionTypeId).HasColumnName("QuestionType_ID");
            entity.Property(e => e.TestId).HasColumnName("Test_ID");
            entity.Property(e => e.TestQuest).HasColumnName("Test_Quest");

            entity.HasOne(d => d.QuestionType).WithMany(p => p.Questions)
                .HasForeignKey(d => d.QuestionTypeId)
                .HasConstraintName("FK_Questions_D_QuestionTypes");

            entity.HasOne(d => d.Test).WithMany(p => p.Questions)
                .HasForeignKey(d => d.TestId)
                .HasConstraintName("FK_Questions_Tests");
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CuratorCreateId).HasColumnName("Curator_Create_ID");
            entity.Property(e => e.DateOfCreating).HasColumnType("datetime");
            entity.Property(e => e.PenaltyMax).HasColumnName("Penalty_Max");
            entity.Property(e => e.TestTypeId).HasColumnName("TestType_ID");
            entity.Property(e => e.TopicId).HasColumnName("Topic_ID");

            entity.HasOne(d => d.CuratorCreate).WithMany(p => p.TestsNavigation)
                .HasForeignKey(d => d.CuratorCreateId)
                .HasConstraintName("FK_Tests_Curators");

            entity.HasOne(d => d.TestType).WithMany(p => p.Tests)
                .HasForeignKey(d => d.TestTypeId)
                .HasConstraintName("FK_Tests_D_TestTypes");

            entity.HasOne(d => d.Topic).WithMany(p => p.Tests)
                .HasForeignKey(d => d.TopicId)
                .HasConstraintName("FK_Tests_Topics");
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Subject");

            entity.ToTable(tb => tb.HasComment("Темы тестов"));

            entity.Property(e => e.Id).HasColumnName("ID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
