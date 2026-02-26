using System;
using System.Collections.Generic;

namespace RegistrationTeacherSafetyWheel.Models;

public partial class Teacher
{
    public int Id { get; set; }

    public string? Login { get; set; }

    public string? Password { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
}
