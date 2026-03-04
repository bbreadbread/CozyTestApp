using System;
using System.Collections.Generic;

namespace RegistrationCuratorSafetyWheel.Models;

public partial class DTestType
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public int? TimeLimitSecond { get; set; }

    public virtual ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();
}
