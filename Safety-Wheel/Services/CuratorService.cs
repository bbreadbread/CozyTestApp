using Microsoft.EntityFrameworkCore;
using CozyTest.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CozyTest.Services
{
    public class CuratorService
    {
        private readonly CozyTestContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Curator> Curators { get; set; } = new();

        public CuratorService()
        {
            GetAll();
        }

        public void Add(Curator cur)
        {
            var _cur = new Curator
            {
                Login = cur.Login,
                Password = cur.Password,
                Name = cur.Name,
                IsAdmin = cur.IsAdmin,
                IsArchive = cur.IsArchive,

            };
            _db.Add(_cur);
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

        public void GetAll(int userId)
        {
            var teachers = _db.Curators
                .Include(t => t.Participants)
                .Where(o=>o.Participants.Any(p=>p.Id == userId))
                .ToList();
            Curators.Clear();

            foreach (var teacher in teachers)
            {
                Curators.Add(teacher);
            }
        }
        public Curator GetLast()
        {
            var query = _db.Curators
                .OrderByDescending(a => a.Id)
                      .FirstOrDefault();

            return query;
        }
        public Curator GetById(int id)
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

        public void UpdateCuratorArchiveStatus(int userId)
        {
            try
            {
                var user = _db.Curators
                    .FirstOrDefault(ug => ug.Id == userId);

                if (user != null)
                {
                    user.IsArchive = !user.IsArchive;
                    if (user.IsArchive == null) user.IsArchive = true;
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
        public void UpdateCuratorAdminStatus(int userId)
        {
            try
            {
                var user = _db.Curators
                    .FirstOrDefault(ug => ug.Id == userId);

                if (user != null)
                {
                    user.IsAdmin = !user.IsAdmin;
                    if (user.IsAdmin == null) user.IsAdmin = true;

                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }



    }
}
