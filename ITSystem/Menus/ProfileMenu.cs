using ITSystem.Data.Entities;
using ITSystem.Services;

namespace ITSystem.Menus
{
    internal class ProfileMenu : IMenu
    {
        private readonly User _currentUser;
        private readonly IUserService _userService;

        public ProfileMenu(User currentUser, IUserService userService)
        {
            _currentUser = currentUser;
            _userService = userService;
        }

        public void Run()
        {
            string? input;
            do
            {
                Console.Clear();
                Console.WriteLine("== Min profil ==");
                Console.WriteLine($"Användarnamn: {_currentUser.Username}");
                Console.WriteLine($"Roll: {_currentUser.Role}");
                Console.WriteLine();
                Console.WriteLine("1. Ändra användarnamn");
                Console.WriteLine("2. Ändra lösenord");
                Console.WriteLine("0. Tillbaka");
                Console.Write("Val: ");
                input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        ChangeUsername();
                        break;
                    case "2":
                        ChangePassword();
                        break;
                    case "0":
                        break;
                    default:
                        Console.WriteLine("Ogiltigt val.");
                        Pause();
                        break;
                }
            } while (input != "0");
        }

        private void ChangeUsername()
        {
            Console.Write("Nytt användarnamn: ");
            var newUsername = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(newUsername))
            {
                _currentUser.Username = newUsername;
                _userService.UpdateUser(_currentUser);
                Console.WriteLine("Användarnamn uppdaterat.");
            }
            else
            {
                Console.WriteLine("Ogiltigt användarnamn.");
            }

            Pause();
        }

        private void ChangePassword()
        {
            Console.Write("Nytt lösenord: ");
            var newPassword = ReadPassword();

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                _currentUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                _userService.UpdateUser(_currentUser);
                Console.WriteLine("Lösenord uppdaterat.");
            }
            else
            {
                Console.WriteLine("Ogiltigt lösenord.");
            }

            Pause();
        }

        private void Pause()
        {
            Console.WriteLine("\nTryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
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
