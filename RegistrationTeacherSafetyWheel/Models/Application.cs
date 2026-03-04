using System;
using System.Collections.Generic;

namespace RegistrationCuratorCozyTest.Models;

public partial class Application
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Login { get; set; }

    public string? Password { get; set; }

    public string? Status { get; set; }

    public DateTime? DateTimeApplication { get; set; }

    public int? ReviewerId { get; set; }

    public virtual Curator? Reviewer { get; set; }
}
