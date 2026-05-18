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

        private static bool _adminModeOn;
        public static bool AdminModeOn
        {
            get => _adminModeOn;
            set
            {
                _adminModeOn = value;
                AdminModeOnChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static event EventHandler? AdminModeOnChanged;

        public static void Clear()
        {
            ClassUser = null;
            TypeUser = 0;
            Id = 0;
            Name = string.Empty;
            Login = string.Empty;
            AdminModeOn = false;
        }
    }
}
