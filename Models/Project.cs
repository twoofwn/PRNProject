using System;
using System.Collections.Generic;

namespace PRNProject.Models;

public partial class Project
{
    public int ProjectId { get; set; }

    public int OwnerUserId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? ColorHex { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User OwnerUser { get; set; } = null!;

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
