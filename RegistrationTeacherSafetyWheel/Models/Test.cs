using System;
using System.Collections.Generic;

namespace RegistrationTeacherSafetyWheel.Models;

public partial class Test
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? SubjectId { get; set; }

    public int? TeacherId { get; set; }

    public int? PenaltyMax { get; set; }

    public int? MaxScore { get; set; }

    public DateTime? DateOfCreating { get; set; }

    public bool? IsPublic { get; set; }

    public virtual ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual Subject? Subject { get; set; }

    public virtual Teacher? Teacher { get; set; }
}
