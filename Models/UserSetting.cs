using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRNProject.Models
{
    [Table("UserSetting")]
    public partial class UserSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] 
        public int UserId { get; set; }

        [StringLength(20)]
        public string Theme { get; set; }

        [StringLength(10)]
        public string Language { get; set; }

        public DateTime? LastLogin { get; set; }

        // Tạo mối quan hệ 1-1 với User
        [ForeignKey(nameof(UserId))]
        [InverseProperty("UserSetting")]
        public virtual User User { get; set; }
    }
}