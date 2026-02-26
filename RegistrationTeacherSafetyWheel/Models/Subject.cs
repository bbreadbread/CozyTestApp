using System;
using System.Collections.Generic;

namespace RegistrationTeacherSafetyWheel.Models;

public partial class Subject
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
}
