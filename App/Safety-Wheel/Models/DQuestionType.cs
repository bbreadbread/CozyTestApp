using System;
using System.Collections.Generic;

namespace CozyTest.Models;

public partial class DQuestionType
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
