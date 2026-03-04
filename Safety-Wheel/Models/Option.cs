using System;
using System.Collections.Generic;

namespace Safety_Wheel.Models;

public partial class Option
{
    public int Id { get; set; }

    public int QuestionId { get; set; }

    public int? Number { get; set; }

    public string? TextAnswer { get; set; }

    public bool? IsCorrect { get; set; }

    public virtual ICollection<ParticipantAnswer> ParticipantAnswers { get; set; } = new List<ParticipantAnswer>();

    public virtual Question? Question { get; set; }
}
