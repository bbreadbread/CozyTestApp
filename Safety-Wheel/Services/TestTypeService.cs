using Microsoft.EntityFrameworkCore;
using Safety_Wheel.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Safety_Wheel.Services
{
    public class DTestTypeService
    {
        private readonly CozyTestContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<DTestType> DTestTypes { get; set; } = new();

        public DTestTypeService(int? type = null)
        {
            GetAll();
        }

        public int Commit() => _db.SaveChanges();

        public void GetAll()
        {
            var query = _db.DTestTypes.ToList();

            foreach (var testType in query)
            {
                DTestTypes.Add(testType);
            }
        }
        public DTestType GetTypeById(int? type)
        {
            var testType = DTestTypes
                .FirstOrDefault(t => t.Id == type);

            return testType;
        }
    }
}
