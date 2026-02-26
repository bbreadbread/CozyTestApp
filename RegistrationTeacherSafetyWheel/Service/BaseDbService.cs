using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RegistrationTeacherSafetyWheel.Models;

namespace RegistrationTeacherSafetyWheel.Service
{
    public class BaseDbService
    {
        private static BaseDbService? instance;
        private SafetyWheelContext _context;
        public SafetyWheelContext Context => _context;
        private BaseDbService()
        {
            _context = new SafetyWheelContext();
        }
        public void SetContext(SafetyWheelContext context)
        {
            _context = context;
        }
        public static BaseDbService Instance
        {
            get
            {
                if (instance == null)
                    instance = new BaseDbService();
                return instance;
            }
        }
    }
}
