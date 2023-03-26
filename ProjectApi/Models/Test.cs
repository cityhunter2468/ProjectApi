using System;
using System.Collections.Generic;

namespace ProjectApi.Models
{
    public partial class Test
    {
        public Test()
        {
            Questions = new HashSet<Question>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreateAt { get; set; }
        public bool Mode { get; set; }
        public int Duration { get; set; }
        public int NumberOfTimes { get; set; }
        public int AccountId { get; set; }
        public int FolderId { get; set; }

        public virtual Account Account { get; set; } = null!;
        public virtual ICollection<Question> Questions { get; set; }
    }
}
