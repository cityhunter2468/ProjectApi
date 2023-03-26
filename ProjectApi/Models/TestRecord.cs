using System;
using System.Collections.Generic;

namespace ProjectApi.Models
{
    public partial class TestRecord
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public string TestRecord1 { get; set; } = null!;
        public string QuesRecord { get; set; } = null!;
        public string? AnsRecord { get; set; }
        public DateTime CreateAt { get; set; }
        public int? Mark { get; set; }
        public int AccountId { get; set; }

        public virtual Account Account { get; set; } = null!;
        public virtual Test Test { get; set; } = null!;
    }
}
