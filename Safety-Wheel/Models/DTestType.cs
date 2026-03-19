using System;
using System.Collections.Generic;

namespace CozyTest.Models;

/// <summary>
/// Тест, опросник
/// </summary>
public partial class DTestType
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
}
