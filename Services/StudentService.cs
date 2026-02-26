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
    public class StudentService
    {
        private readonly SafetyWheelContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Student> Students { get; set; } = new();

        public StudentService()
        {
            GetAllStudents();
        }

        public void Add(Student student)
        {
            var _student = new Student
            {
                Name = student.Name,
                Login = student.Login,
                Password = student.Password,
                TeachersId = student.TeachersId
            };
            _db.Add(_student);
            Students.Add(_student);
            Commit();
        }

        public int Commit() => _db.SaveChanges();

        public void GetAllStudents(int? teacherId = null)
        {
            IQueryable<Student> query = _db.Students
                .Include(s => s.Teachers)
                .Include(s => s.Attempts);

            if (teacherId != null)
                query = query.Where(s => s.TeachersId == teacherId);

            var students = query.ToList();
            Students.Clear();

            foreach (var student in students)
            {
                Students.Add(student);
            }
        }

        public void ReloadStudents(int teacherId)
        {
            var stud  = _db.Students
                         .Where(s => s.TeachersId == teacherId)
                         .ToList();
            Students.Clear();
            foreach (var student in stud)
            {
                Students.Add(student);
            }
        }

        public Student? GetCurrentStudent(int? studentId = null)
        {
            if (!studentId.HasValue) return null;

            return Students
                      .FirstOrDefault(s => s.Id == studentId.Value);
        }

        public void Remove(Student student)
        {
                _db.Remove(student);
                if (Commit() > 0)
                    if (Students.Contains(student))
                        Students.Remove(student);
        }

        public void Update(Student student)
        {
            var existing = _db.Students.Find(student.Id);
            if (existing != null)
            {
                existing.Name = student.Name;
                existing.Login = student.Login;
                existing.Password = student.Password;
                existing.TeachersId = student.TeachersId;
                Commit();
            }
        }
    }
}
