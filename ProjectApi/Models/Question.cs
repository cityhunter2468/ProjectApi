using System;
using System.Collections.Generic;

namespace ProjectApi.Models
{
    public partial class Question
    {
        public int Id { get; set; }
        public bool Mode { get; set; }
        public string Ques { get; set; } = null!;
        public string Ans { get; set; } = null!;
        public int? CorrectAns { get; set; }
        public DateTime CreateAt { get; set; }
        public int AccountId { get; set; }
        public int TestId { get; set; }

        public virtual Account Account { get; set; } = null!;
        public virtual Test Test { get; set; } = null!;
    }
}
