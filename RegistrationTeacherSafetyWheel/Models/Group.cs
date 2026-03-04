using System;
using System.Collections.Generic;

namespace RegistrationCuratorCozyTest.Models;

public partial class Group
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? CuratorId { get; set; }

    public string? Description { get; set; }

    public virtual Curator? Curator { get; set; }

    public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();
}
