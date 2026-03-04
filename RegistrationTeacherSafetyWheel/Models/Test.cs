using System;
using System.Collections.Generic;

namespace RegistrationCuratorCozyTest.Models;

public partial class Test
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? TopicId { get; set; }

    public int? CuratorCreateId { get; set; }

    public int? PenaltyMax { get; set; }

    public DateTime? DateOfCreating { get; set; }

    public int? TestTypeId { get; set; }

    public int? TimeLimitSecond { get; set; }

    public string? Description { get; set; }

    public bool? IsPublic { get; set; }

    public bool? IsArchive { get; set; }

    public int? MaxNumPassing { get; set; }

    public virtual ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();

    public virtual Curator? CuratorCreate { get; set; }

    public virtual ICollection<ParticipantsAssignedTest> ParticipantsAssignedTests { get; set; } = new List<ParticipantsAssignedTest>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual DTestType? TestType { get; set; }

    public virtual Topic? Topic { get; set; }

    public virtual ICollection<Curator> Curators { get; set; } = new List<Curator>();

    public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();
}
