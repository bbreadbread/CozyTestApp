using System;
using System.Collections.Generic;

namespace RegistrationTeacherSafetyWheel.Models;

public partial class TestType
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public int? TimeLimitSecond { get; set; }

    public virtual ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();
}
