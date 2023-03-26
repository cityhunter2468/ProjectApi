using System;
using System.Collections.Generic;

namespace ProjectApi.Models
{
    public partial class Account
    {
        public Account()
        {
            AccountCourses = new HashSet<AccountCourse>();
            //Courses = new HashSet<Course>();
            Questions = new HashSet<Question>();
            Tests = new HashSet<Test>();
        }

        public int Id { get; set; }
        public string? DisplayName { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int Role { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }

        public virtual ICollection<AccountCourse> AccountCourses { get; set; }
        //public virtual ICollection<Course> Courses { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<Test> Tests { get; set; }
    }
}
