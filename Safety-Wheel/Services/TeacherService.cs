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
    public class TeacherService
    {
        private readonly SafetyWheelContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Teacher> Teachers { get; set; } = new();

        public TeacherService()
        {
            GetAll();
        }

        public void Add(Teacher teacher)
        {
            var _teacher = new Teacher
            {
                Login = teacher.Login,
                Password = teacher.Password,
                Name = teacher.Name
            };
            _db.Add(_teacher);
            Commit();
        }

        public int Commit() => _db.SaveChanges();

        public void GetAll()
        {
            var teachers = _db.Teachers
                .Include(t => t.Students)
                .Include(t => t.Tests)
                .ToList();
            Teachers.Clear();

            foreach (var teacher in teachers)
            {
                Teachers.Add(teacher);
            }
        }

        public Teacher GetTeacherById(int id)
        {
            var tea = Teachers.Where(q=> q.Id == id).FirstOrDefault();
            return tea;
        }

        public void Remove(Teacher teacher)
        {
            _db.Remove(teacher);
            if (Commit() > 0)
                if (Teachers.Contains(teacher))
                    Teachers.Remove(teacher);
        }

        public void Update(Teacher teacher)
        {
            var existing = _db.Teachers.Find(teacher.Id);
            if (existing != null)
            {
                existing.Login = teacher.Login;
                existing.Password = teacher.Password;
                existing.Name = teacher.Name;
                Commit();
            }
        }

        public bool UserExistsByLogin(string login)
        {
            return _db.Teachers.Any(t => t.Login == login)
                || _db.Students.Any(s => s.Login == login);
        }
    }
}
