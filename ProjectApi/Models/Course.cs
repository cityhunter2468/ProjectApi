using System;
using System.Collections.Generic;

namespace ProjectApi.Models
{
    public partial class Course
    {
        public Course()
        {
            AccountCourses = new HashSet<AccountCourse>();
            Folders = new HashSet<Folder>();
        }

        public int Id { get; set; }
        public string CourseName { get; set; } = null!;
        public string CourseDescription { get; set; } = null!;
        public DateTime CreateAt { get; set; }
        public int AccountId { get; set; }

        public virtual Account Account { get; set; } = null!;
        public virtual ICollection<AccountCourse> AccountCourses { get; set; }
        public virtual ICollection<Folder> Folders { get; set; }
    }
}
