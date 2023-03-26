using System;
using System.Collections.Generic;

namespace ProjectApi.Models
{
    public partial class AccountCourse
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int CourseId { get; set; }
        public int Status { get; set; }

        public virtual Account Account { get; set; } = null!;
        public virtual Course Course { get; set; } = null!;
    }
}
