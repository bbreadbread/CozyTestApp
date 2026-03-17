using System;
using System.Collections.Generic;

namespace Safety_Wheel.Models;

public partial class Test
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? TopicId { get; set; }

    public int? CuratorCreateId { get; set; }

    public int? PenaltyMax { get; set; }

    public DateTime? DateOfCreating { get; set; }

    public int? DTestTypeId { get; set; }

    public int? TimeLimitSecond { get; set; }

    public string? Description { get; set; }

    public bool? IsPublic { get; set; }

    public bool? IsArchive { get; set; }

    public int? MaxNumPassing { get; set; }

    public int? CriteriaId { get; set; }

    public virtual ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();
    public virtual Criterion? Criteria { get; set; }

    public virtual Curator? CuratorCreate { get; set; }

    public virtual ICollection<BParticipantAssignedTest> BParticipantAssignedTest { get; set; } = new List<BParticipantAssignedTest>();
    public virtual ICollection<BParticipantFavoriteTest> BParticipantFavoriteTest { get; set; } = new List<BParticipantFavoriteTest>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual DTestType? DTestType { get; set; }

    public virtual Topic? Topic { get; set; }

    public virtual ICollection<Curator> Curators { get; set; } = new List<Curator>();

    public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();
}
