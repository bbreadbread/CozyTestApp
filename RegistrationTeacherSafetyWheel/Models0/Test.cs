using System;
using System.Collections.Generic;

namespace RegistrationCuratorSafetyWheel.Models;

public partial class Test
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? TopicId { get; set; }

    public int? CuratorId { get; set; }

    public int? PenaltyMax { get; set; }

    public int? MaxScore { get; set; }

    public DateTime? DateOfCreating { get; set; }

    public bool? IsPublic { get; set; }

    public virtual ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual Topic? Topic { get; set; }

    public virtual Curator? Curator { get; set; }
}
