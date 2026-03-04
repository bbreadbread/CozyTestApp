using System;
using System.Collections.Generic;

namespace RegistrationCuratorCozyTest.Models;

/// <summary>
/// Темы тестов
/// </summary>
public partial class Topic
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
}
