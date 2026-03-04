using System;
using System.Collections.Generic;

namespace RegistrationCuratorSafetyWheel.Models;

public partial class Participant
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Login { get; set; }

    public string? Password { get; set; }

    public int? CuratorCreateId { get; set; }

    public virtual ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();

    public virtual Curator? Curators { get; set; }
}
