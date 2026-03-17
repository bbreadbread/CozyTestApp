using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Safety_Wheel.Models;

public partial class CozyTestContext : DbContext
{
    public CozyTestContext()
    {
    }

    public CozyTestContext(DbContextOptions<CozyTestContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Requests> Requests { get; set; }

    public virtual DbSet<Attempt> Attempts { get; set; }

    public virtual DbSet<Curator> Curators { get; set; }

    public virtual DbSet<DQuestionType> DQuestionTypes { get; set; }

    public virtual DbSet<DTestType> DTestTypes { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<Participant> Participants { get; set; }

    public virtual DbSet<ParticipantAnswer> ParticipantAnswers { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Test> Tests { get; set; }

    public virtual DbSet<Topic> Topics { get; set; }

    public virtual DbSet<BParticipantFavoriteTest> BParticipantFavoriteTest { get; set; }
    //public virtual DbSet<BParticipantAssignedTest> BParticipantAssignedTest { get; set; }
    //public virtual DbSet<BCuratorsParticipant> BCuratorsParticipant { get; set; }
    //public virtual DbSet<BGroupsParticipant> BGroupsParticipant { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer("Server=HOME-PC;Database=cozy-test;Trusted_Connection=True;TrustServerCertificate=True");
    }
        

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<Requests>(entity =>
        {
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.DateTimeApplication).HasColumnType("datetime");
            entity.Property(e => e.ReviewerId).HasColumnName("Reviewer_ID");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.Requests)
                .HasForeignKey(d => d.ReviewerId)
                .HasConstraintName("FK_Requests_Curators");
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
            entity.HasKey(e => e.Id).HasName("PK_Curators");

            entity.Property(e => e.Id).HasColumnName("ID");

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
            entity.HasKey(e => e.Id).HasName("PK_DTestType");

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
            entity.HasKey(e => e.Id).HasName("PK_Participants");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CuratorCreateId).HasColumnName("Curator_Create_ID");

            entity.HasOne(d => d.CuratorCreate).WithMany(p => p.ParticipantsNavigation)
                .HasForeignKey(d => d.CuratorCreateId)
                .HasConstraintName("FK_Participants_Curators1");

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

        modelBuilder.Entity<BParticipantFavoriteTest>(entity =>
        {
            entity.HasKey(e => new { e.ParticipantId, e.TestId });

            entity.ToTable("Participants_Favorite_Tests");


            entity.HasOne(d => d.Participant)
                .WithMany()
                .HasForeignKey(d => d.ParticipantId);

            entity.HasOne(d => d.Test)
                .WithMany()
                .HasForeignKey(d => d.TestId);
        });

        //modelBuilder.Entity<BParticipantAssignedTest>(entity =>
        //{
        //    entity.HasKey(e => new { e.ParticipantId, e.TestId, e.CuratorId });

        //    entity.ToTable("Participants_Assigned_Tests");

        //    entity.Property(e => e.DateTimeAssigned).HasColumnType("datetime");

        //    entity.HasOne(d => d.Participant)
        //        .WithMany()
        //        .HasForeignKey(d => d.ParticipantId);

        //    entity.HasOne(d => d.Test)
        //        .WithMany()
        //        .HasForeignKey(d => d.TestId);

        //    entity.HasOne(d => d.Curator)
        //        .WithMany()
        //        .HasForeignKey(d => d.CuratorId);
        //});

        //modelBuilder.Entity<BGroupsParticipant>(entity =>
        //{
        //    entity.HasKey(e => new { e.GroupId, e.ParticipantId });

        //    entity.ToTable("Groups_Participants");

        //    entity.Property(e => e.GroupId)
        //        .HasColumnName("Group_ID");

        //    entity.Property(e => e.ParticipantId)
        //        .HasColumnName("Participant_ID");

        //    entity.HasOne(d => d.Group)
        //        .WithMany(p => p.BGroupsParticipant)
        //        .HasForeignKey(d => d.GroupId)
        //        .OnDelete(DeleteBehavior.ClientSetNull)
        //        .HasConstraintName("FK_Groups_Participants_Groups");

        //    entity.HasOne(d => d.Participant)
        //        .WithMany(p => p.BGroupsParticipant)
        //        .HasForeignKey(d => d.ParticipantId)
        //        .OnDelete(DeleteBehavior.ClientSetNull)
        //        .HasConstraintName("FK_Groups_Participants_Participants");
        //});

        //modelBuilder.Entity<BCuratorsParticipant>(entity =>
        //{
        //    entity.HasKey(e => new { e.CuratorId, e.ParticipantId });
        //    entity.ToTable("Curators_Participants");

        //    entity.HasOne(d => d.Participant)
        //        .WithMany(p => p.BCuratorsParticipant)
        //        .HasForeignKey(d => d.ParticipantId);

        //    entity.HasOne(d => d.Curator)
        //        .WithMany(p => p.BCuratorsParticipant)
        //        .HasForeignKey(d => d.CuratorId);
        //});



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
            entity.Property(e => e.DTestTypeId).HasColumnName("TestType_ID");
            entity.Property(e => e.TopicId).HasColumnName("Topic_ID");

            entity.HasOne(d => d.Criteria).WithMany(p => p.Tests)
                .HasForeignKey(d => d.CriteriaId)
                .HasConstraintName("FK_Tests_Criteria");

            entity.HasOne(d => d.CuratorCreate).WithMany(p => p.TestsNavigation)
                .HasForeignKey(d => d.CuratorCreateId)
                .HasConstraintName("FK_Tests_Curators");

            entity.HasOne(d => d.DTestType).WithMany(p => p.Tests)
                .HasForeignKey(d => d.DTestTypeId)
                .HasConstraintName("FK_Tests_D_TestTypes");

            entity.HasOne(d => d.Topic).WithMany(p => p.Tests)
                .HasForeignKey(d => d.TopicId)
                .HasConstraintName("FK_Tests_Topics");
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Topic");

            entity.ToTable(tb => tb.HasComment("Темы тестов"));

            entity.Property(e => e.Id).HasColumnName("ID");
        });

        modelBuilder.Entity<UserActionLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Logs");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Object)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.TimeStamp).HasColumnType("datetime");
            entity.Property(e => e.WhoMade)
                .HasMaxLength(10)
                .IsFixedLength();
        });

        OnModelCreatingPartial(modelBuilder);


    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
