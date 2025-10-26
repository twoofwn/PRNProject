using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRNProject.Models
{
    public static class AppSession
    {
        public static User CurrentUser { get; set; }
        public static DateTime? PreviousLastLogin { get; set; }
    }
}
