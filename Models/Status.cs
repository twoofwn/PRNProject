using System;
using System.Collections.Generic;

namespace PRNProject.Models;

public partial class Status
{
    public int StatusId { get; set; }

    public string Name { get; set; } = null!;

    public bool IsFinal { get; set; }

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
