using System;
using System.Collections.Generic;

namespace ProjectApi.Models
{
    public partial class Folder
    {
        public Folder()
        {
            Files = new HashSet<File>();
        }

        public int Id { get; set; }
        public string FolderName { get; set; } = null!;
        public int CourseId { get; set; }
        public DateTime? CreateAt { get; set; }
        public int AccountId { get; set; }

        public virtual Course Course { get; set; } = null!;
        public virtual ICollection<File> Files { get; set; }
    }
}
