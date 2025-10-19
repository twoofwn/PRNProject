using System;
using System.Collections.Generic;

namespace PRNProject.Models;

public partial class Priority
{
    public int PriorityId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
