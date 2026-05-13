using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CozyTest.Models
{
    public class Сorrespondence
    {
        public int ConstantId { get; set; }      
        public int СorrespondingId { get; set; } 

        public virtual Option Constant { get; set; }
        public virtual Option Corresponding { get; set; }
    }
}
