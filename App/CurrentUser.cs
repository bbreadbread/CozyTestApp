using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safety_Wheel
{
    public static class CurrentUser
    {
        public static int Id { get; set; }
        public static string Name { get; set; }
        public static string UserType { get; set; }
        public static string Login { get; set; }

        public static void Clear()
        {
            Id = 0;
            Name = string.Empty;
            UserType = string.Empty;
        }
    }
}
