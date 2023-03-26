using System;
using System.Collections.Generic;

namespace ProjectApi.Models
{
    public partial class File
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreateAt { get; set; }
        public string? Url { get; set; }
        public int FolderId { get; set; }
        public int AccountId { get; set; }

        public virtual Folder Folder { get; set; } = null!;
    }
}
