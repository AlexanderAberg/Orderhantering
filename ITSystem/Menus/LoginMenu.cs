using ITSystem.Data.Contexts;
using ITSystem.Data.Entities;
using System;
using System.Linq;
using System.Threading;

namespace ITSystem.Menus
{
    internal class LoginMenu
    {
        private readonly OrderDbContext _dbContext;

        public LoginMenu(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public User? Login()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("== Logga in ==");

                Console.Write("Användarnamn: ");
                var username = Console.ReadLine();

                Console.Write("Lösenord: ");
                var password = ReadPassword();

                var user = _dbContext.Users.FirstOrDefault(u => u.Username == username);

                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    Console.WriteLine("Inloggning lyckades!");
                    Thread.Sleep(1500);
                    return user;
                }
                else
                {
                    Console.WriteLine("Fel användarnamn eller lösenord.");
                    Thread.Sleep(1500);
                }
            }
        }

        private string ReadPassword()
        {
            var password = string.Empty;
            ConsoleKey key;

            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && password.Length > 0)
                {
                    Console.Write("\b \b");
                    password = password[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    password += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);

            Console.WriteLine();
            return password;
        }
    }
}
