using ITSystem.Data.Entities;
using ITSystem.Services;

namespace ITSystem.Menus
{
    internal class UserAdminMenu : IMenu
    {
        private readonly IUserService _userService;

        public UserAdminMenu(User currentUser, Data.Contexts.OrderDbContext dbContext, IUserService userService)
        {
            _userService = userService;
        }

        public void Run()
        {
            string? input;
            do
            {
                Console.Clear();
                Console.WriteLine("== Användaradministration ==");
                Console.WriteLine("1. Lista alla användare");
                Console.WriteLine("2. Lägg till användare");
                Console.WriteLine("3. Redigera användare");
                Console.WriteLine("4. Ta bort användare");
                Console.WriteLine("0. Tillbaka");
                Console.Write("Val: ");
                input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        ListUsers();
                        break;
                    case "2":
                        CreateUser();
                        break;
                    case "3":
                        EditUser();
                        break;
                    case "4":
                        DeleteUser();
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

        private void ListUsers()
        {
            Console.Clear();
            var users = _userService.GetAllUsers();
            Console.WriteLine("== Alla användare ==");
            foreach (var user in users)
            {
                Console.WriteLine($"ID: {user.Id}, Användarnamn: {user.Username}, Roll: {user.Role}");
            }
            Pause();
        }

        private void CreateUser()
        {
            Console.Clear();
            Console.WriteLine("== Skapa ny användare ==");

            Console.Write("Användarnamn: ");
            var username = Console.ReadLine();

            Console.Write("Lösenord: ");
            var password = ReadPassword();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Användarnamn och lösenord får inte vara tomma.");
                Pause();
                return;
            }

            if (_userService.GetAllUsers().Any(u => u.Username == username))
            {
                Console.WriteLine("Användarnamnet är redan taget.");
                Pause();
                return;
            }

            Console.Write("Roll (User/Admin): ");
            var role = Console.ReadLine();

            if (role != "User" && role != "Admin")
            {
                Console.WriteLine("Ogiltig roll. Standard 'User' används.");
                role = "User";
            }

            _userService.Register(username, password, role);

            Console.WriteLine("Användare skapad!");
            Pause();
        }


        private void EditUser()
        {
            Console.Clear();
            Console.WriteLine("== Redigera användare ==");

            Console.Write("Ange användar-ID att redigera: ");
            if (!int.TryParse(Console.ReadLine(), out int userId))
            {
                Console.WriteLine("Ogiltigt ID.");
                Pause();
                return;
            }

            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                Console.WriteLine("Användaren hittades inte.");
                Pause();
                return;
            }

            Console.WriteLine($"Nuvarande användarnamn: {user.Username}");
            Console.Write("Nytt användarnamn (tomt för att behålla): ");
            var newUsername = Console.ReadLine();

            Console.WriteLine("Vill du ändra lösenord? (j/n): ");
            var changePassword = Console.ReadLine();

            string? newPassword = null;
            if (changePassword?.ToLower() == "j")
            {
                Console.Write("Nytt lösenord: ");
                newPassword = ReadPassword();
            }

            Console.WriteLine($"Nuvarande roll: {user.Role}");
            Console.Write("Ny roll (Admin/User, tomt för att behålla): ");
            var newRole = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(newUsername))
                user.Username = newUsername;

            if (!string.IsNullOrWhiteSpace(newPassword))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            if (!string.IsNullOrWhiteSpace(newRole))
                user.Role = newRole;

            _userService.UpdateUser(user);
            Console.WriteLine("Användare uppdaterad.");
            Pause();
        }

        private void DeleteUser()
        {
            Console.Clear();
            Console.WriteLine("== Ta bort användare ==");

            Console.Write("Ange användar-ID att ta bort: ");
            if (!int.TryParse(Console.ReadLine(), out int userId))
            {
                Console.WriteLine("Ogiltigt ID.");
                Pause();
                return;
            }

            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                Console.WriteLine("Användaren hittades inte.");
                Pause();
                return;
            }

            Console.Write($"Är du säker på att du vill ta bort användare '{user.Username}'? (j/n): ");
            var confirm = Console.ReadLine();
            if (confirm?.ToLower() == "j")
            {
                _userService.DeleteUser(userId);
                Console.WriteLine("Användare borttagen.");
            }
            else
            {
                Console.WriteLine("Åtgärd avbruten.");
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
