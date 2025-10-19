using System;
using System.Collections.Generic;

namespace PRNProject.Models;

public partial class Comment
{
    public int CommentId { get; set; }

    public int TaskId { get; set; }

    public int AuthorUserId { get; set; }

    public string Body { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User AuthorUser { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
