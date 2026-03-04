using System;
using System.Collections.Generic;

namespace RegistrationCuratorCozyTest.Models;

public partial class Question
{
    public int Id { get; set; }

    public int? TestId { get; set; }

    public int? Number { get; set; }

    public string? TestQuest { get; set; }

    public string? PicturePath { get; set; }

    public int? QuestionTypeId { get; set; }

    public string? Comments { get; set; }

    public virtual ICollection<Option> Options { get; set; } = new List<Option>();

    public virtual ICollection<ParticipantAnswer> ParticipantAnswers { get; set; } = new List<ParticipantAnswer>();

    public virtual DQuestionType? QuestionType { get; set; }

    public virtual Test? Test { get; set; }
}
