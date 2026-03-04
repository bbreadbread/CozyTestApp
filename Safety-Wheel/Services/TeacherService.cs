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
    public class CuratorService
    {
        private readonly CozyTestContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Curator> Curators { get; set; } = new();

        public CuratorService()
        {
            GetAll();
        }

        public void Add(Curator teacher)
        {
            var _teacher = new Curator
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
            var teachers = _db.Curators
                .Include(t => t.Participants)
                .Include(t => t.Tests)
                .ToList();
            Curators.Clear();

            foreach (var teacher in teachers)
            {
                Curators.Add(teacher);
            }
        }

        public Curator GetCuratorById(int id)
        {
            var tea = Curators.Where(q=> q.Id == id).FirstOrDefault();
            return tea;
        }

        public void Remove(Curator teacher)
        {
            _db.Remove(teacher);
            if (Commit() > 0)
                if (Curators.Contains(teacher))
                    Curators.Remove(teacher);
        }

        public void Update(Curator teacher)
        {
            var existing = _db.Curators.Find(teacher.Id);
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
            return _db.Curators.Any(t => t.Login == login)
                || _db.Participants.Any(s => s.Login == login);
        }
    }
}
