using System;
using System.Collections.Generic;

namespace RegistrationCuratorCozyTest.Models;

public partial class ParticipantAnswer
{
    public int AttemptId { get; set; }

    public int QuestionId { get; set; }

    public int OptionId { get; set; }

    public bool? IsCorrect { get; set; }

    public DateTime? AnsweredAt { get; set; }

    public virtual Attempt Attempt { get; set; } = null!;

    public virtual Option Option { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;
}
