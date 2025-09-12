using ITSystem.Data.Contexts;
using ITSystem.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSystem.Services
{
    class UserService : IUserService
    {
        private readonly OrderDbContext _db;

        public UserService(OrderDbContext db)
        {
            _db = db;
        }

        public User? Authenticate(string username, string password)
        {
            var user = _db.Users.FirstOrDefault(u => u.Username == username);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return user;

            return null;
        }

        public void Register(string username, string password, string role)
        {
            if (_db.Users.Any(u => u.Username == username))
            {
                Console.WriteLine("Användarnamnet är redan taget.");
                return;
            }

            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { Username = username, PasswordHash = hash, Role = role };
            _db.Users.Add(user);
            _db.SaveChanges();
        }

        public void UpdateUser(User user)
        {
            _db.Users.Update(user);
            _db.SaveChanges();
        }

        public void DeleteUser(int id)
        {
            var user = _db.Users.Find(id);
            if (user != null)
            {
                _db.Users.Remove(user);
                _db.SaveChanges();
            }
        }

        public User? GetUserById(int id) => _db.Users.Find(id);
        public IEnumerable<User> GetAllUsers() => _db.Users.ToList();
    }

}
