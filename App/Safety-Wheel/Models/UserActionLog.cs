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

        public string? TypeWhoMade { get; set; }

        public string? WhoMade { get; set; }

        public int LevelLog { get; set; }

        public string? TypeObject { get; set; }

        public string? Object { get; set; }

        public string Message { get; set; } = null!;
    }
}
