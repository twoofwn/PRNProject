using System;
using System.Collections.Generic;

namespace PRNProject.Models;

public partial class Tag
{
    public int TagId { get; set; }

    public int OwnerUserId { get; set; }

    public string Name { get; set; } = null!;

    public string? ColorHex { get; set; }

    public virtual User OwnerUser { get; set; } = null!;

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
