using System;
using System.Collections.Generic;

namespace Safety_Wheel.Models;

public partial class Attempt
{
    public int Id { get; set; }

    public int? ParticipantId { get; set; }

    public int? TestId { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public int? Score { get; set; }

    public string? Status { get; set; }

    public int? AttemptNumber { get; set; }

    public virtual Participant? Participant { get; set; }

    public virtual ICollection<ParticipantAnswer> ParticipantAnswers { get; set; } = new List<ParticipantAnswer>();

    public virtual Test? Test { get; set; }
}
