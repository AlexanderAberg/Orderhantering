using ITSystem.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSystem.Services
{
    interface IUserService
    {
        User? Authenticate(string username, string password);
        void Register(string username, string password, string role);
        void UpdateUser(User user);
        void DeleteUser(int id);
        User? GetUserById(int id);
        IEnumerable<User> GetAllUsers();
    }

}
