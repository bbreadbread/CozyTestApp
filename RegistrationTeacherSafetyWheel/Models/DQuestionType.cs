using System;
using System.Collections.Generic;

namespace RegistrationCuratorCozyTest.Models;

/// <summary>
/// Картинка + один ответ
/// Много картинок + много ответов
/// Картинка + много ответов
/// </summary>
public partial class DQuestionType
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
