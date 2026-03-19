using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CozyTest.Models
{
    public partial class UserActionLog
    {
        public int Id { get; set; }

        public DateTime? TimeStamp { get; set; }

        public string? WhoMade { get; set; }

        public string LevelLog { get; set; } = null!;

        public string? Object { get; set; }

        public string Message { get; set; } = null!;
    }
}
