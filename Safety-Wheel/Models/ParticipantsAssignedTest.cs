using System;
using System.Collections.Generic;

namespace Safety_Wheel.Models;

public partial class ParticipantsAssignedTest
{
    public int ParticipantId { get; set; }

    public int TestId { get; set; }

    public DateTime? DateTimeAssigned { get; set; }

    public virtual Participant Participant { get; set; } = null!;

    public virtual Test Test { get; set; } = null!;
}
