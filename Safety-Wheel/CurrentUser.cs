using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CozyTest
{
    public static class CurrentUser
    {
        public static object ClassUser { get; set; }
        public static byte TypeUser { get; set; }
        public static int Id { get; set; }
        public static string Name { get; set; }
        public static string Login { get; set; }

        public static void Clear()
        {
            Id = 0;
            Name = string.Empty;
            TypeUser = 0;
        }
    }
}
