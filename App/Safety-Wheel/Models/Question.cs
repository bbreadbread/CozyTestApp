using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CozyTest.Models;

public partial class Question: ObservableObject
{

    public int Id { get; set; }

    public int? TestId { get; set; }

    public int NumberActual { get; set; }
    public int NumberCreate { get; set; }
    [NotMapped]
    public int NumberNow { get; set; }

    public string? TestQuest { get; set; }

    public string? PicturePath { get; set; }

    public int? QuestionTypeId { get; set; }

    public DateTime? TimeCreate { get; set; }
    public string? Comments { get; set; }
    public bool? IsRandom { get; set; }
    public int Version { get; set; }

    public bool? IsArchive { get; set; }
    public virtual ICollection<Option> Options { get; set; } = new List<Option>();

    public virtual ICollection<ParticipantAnswer> ParticipantAnswers { get; set; } = new List<ParticipantAnswer>();

    public virtual DQuestionType? QuestionType { get; set; }

    public virtual Test? Test { get; set; }

    
}
