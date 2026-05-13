using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CozyTest.Models;

public partial class Option
{
    public int Id { get; set; }

    [Column("Question_ID")]
    public int QuestionId { get; set; }

    public int Number { get; set; }

    public string? TextAnswer { get; set; }

    public bool? IsCorrect { get; set; }
    public bool? IsImage { get; set; }

    [NotMapped]
    public int Version { get; set; }

    public virtual ICollection<ParticipantAnswer> ParticipantAnswers { get; set; } = new List<ParticipantAnswer>();
    public virtual ICollection<Сorrespondence> Сorrespondences { get; set; } = new List<Сorrespondence>();

    [ForeignKey("QuestionId")]
    public virtual Question? Question { get; set; }

    [NotMapped]
    public bool IsConstant { get; set; }

    [NotMapped]
    public int? CorrespondingNumber { get; set; }
}