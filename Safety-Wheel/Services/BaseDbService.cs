using Safety_Wheel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safety_Wheel.Services
{
    public class BaseDbService
    {
        private static BaseDbService? instance;
        private CozyTestContext _context;
        public CozyTestContext Context => _context;
        private BaseDbService()
        {
            _context = new CozyTestContext();
        }
        public void SetContext(CozyTestContext context)
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
