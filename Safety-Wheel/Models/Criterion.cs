using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safety_Wheel.Models
{
    public partial class Criterion
    {
        public int Id { get; set; }

        public int? Excellent { get; set; }

        public int? Good { get; set; }

        public int? Fair { get; set; }

        public int? Poor { get; set; }

        public string? Name { get; set; }

        public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
    }
}
