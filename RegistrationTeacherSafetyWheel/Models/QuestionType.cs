using System;
using System.Collections.Generic;

namespace RegistrationTeacherSafetyWheel.Models;

public partial class QuestionType
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
