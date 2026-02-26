using System;
using System.Collections.Generic;

namespace RegistrationTeacherSafetyWheel.Models;

public partial class Student
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Login { get; set; }

    public string? Password { get; set; }

    public int? TeachersId { get; set; }

    public virtual ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();

    public virtual Teacher? Teachers { get; set; }
}
