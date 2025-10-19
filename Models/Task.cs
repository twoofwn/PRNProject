using System;
using System.Collections.Generic;

namespace PRNProject.Models;

public partial class Task
{
    public int TaskId { get; set; }

    public int? ProjectId { get; set; }

    public int OwnerUserId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int PriorityId { get; set; }

    public int StatusId { get; set; }

    public DateTime? StartAt { get; set; }

    public DateTime? DueAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual User OwnerUser { get; set; } = null!;

    public virtual Priority Priority { get; set; } = null!;

    public virtual Project? Project { get; set; }

    public virtual Status Status { get; set; } = null!;

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
