using Microsoft.EntityFrameworkCore;
using Safety_Wheel.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safety_Wheel.Services
{
    public class SubjectService
    {
        private readonly SafetyWheelContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Subject> Subjects { get; set; } = new();

        public SubjectService()
        {
            GetAll();
        }

        public void Add(Subject subject)
        {
            var _subject = new Subject
            {
                Name = subject.Name
            };
            _db.Add(_subject);
            Commit();
        }

        public int Commit() => _db.SaveChanges();

        public void GetAll()
        {
            var subjects = _db.Subjects
                .Include(s => s.Tests)
                .ToList();
            Subjects.Clear();

            foreach (var subject in subjects)
            {
                Subjects.Add(subject);
            }
        }
    }
}
