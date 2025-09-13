using BCrypt.Net;
using ITSystem.Data.Contexts;
using ITSystem.Data.Entities;
using ITSystem.Services;
using Microsoft.EntityFrameworkCore;

namespace ITSystem
{
    internal class OrderApp
    {
        private readonly OrderDbContext dbContext;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private User? currentUser = null;

        public OrderApp(
            OrderDbContext dbContext,
            IOrderService orderService,
            IUserService userService,
            IProductService productService)
        {
            this.dbContext = dbContext;
            _orderService = orderService;
            _userService = userService;
            _productService = productService;
        }


        internal void Init()
        {
            dbContext.Database.Migrate();

            if (!dbContext.Products.Any())
            {
                dbContext.Products.AddRange(
                    new Product { Name = "Produkt 1", Description = "Beskrivning 1", Price = 100 },
                    new Product { Name = "Produkt 2", Description = "Beskrivning 2", Price = 200 }
                );
            }

            if (!dbContext.Users.Any())
            {
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword("admin123");
                dbContext.Users.Add(new User { Username = "admin", PasswordHash = hashedPassword, Role = "Admin" });
            }

            dbContext.SaveChanges();
        }

        internal void RunMenu()
        {
            Console.Clear();
            Console.WriteLine("== OrderApp ==");

            while (currentUser == null)
                LoginMenu();

            if (currentUser.Role == "Admin")
                AdminMenu();
            else
                UserMenu();
        }

        private void ProductMenu(bool admin = false)
        {
            string? input;
            do
            {
                Console.Clear();
                Console.WriteLine("== Produktmeny ==");

                Console.WriteLine("1. Lista produkter");

                if (admin)
                {
                    Console.WriteLine("2. Skapa ny produkt");
                    Console.WriteLine("3. Uppdatera produkt");
                    Console.WriteLine("4. Ta bort produkt");
                }

                Console.WriteLine("0. Tillbaka");
                Console.Write("Val: ");
                input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        _productService.ListProducts();
                        break;
                    case "2":
                        if (admin)
                            CreateProduct();
                        else
                            Console.WriteLine("Endast administratörer kan skapa produkter.");
                        break;
                    case "3":
                        if (admin)
                            UpdateProduct();
                        else
                            Console.WriteLine("Endast administratörer kan uppdatera produkter.");
                        break;
                    case "4":
                        if (admin)
                            DeleteProduct();
                        else
                            Console.WriteLine("Endast administratörer kan ta bort produkter.");
                        break;
                    case "0":
                        break;
                    default:
                        Console.WriteLine("Ogiltigt val. Försök igen.");
                        break;
                }

            } while (input != "0");
        }


        private void InvalidOption()
        {
            Console.WriteLine("Ogiltigt val.");
            Pause();
        }


        private void OrderMenu(bool admin = false)
        {
            string? input;
            do
            {
                Console.Clear();
                Console.WriteLine("== Ordermeny ==");
                Console.WriteLine("1. Lista ordrar");
                Console.WriteLine("2. Skapa order");

                if (admin)
                {
                    Console.WriteLine("3. Hantera alla ordrar");
                }
                else
                {
                    Console.WriteLine("3. Ändra/Ta bort egna ordrar");
                }

                Console.WriteLine("0. Tillbaka");
                Console.Write("Val: ");
                input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        _orderService.ListOrdersByUser(currentUser.Id);
                        break;
                    case "2":
                        _orderService.CreateOrder(currentUser.Id);
                        break;
                    case "3":
                        if (admin)
                            _orderService.ManageAllOrders(currentUser.Id);
                        else
                            _orderService.ModifyOrDeleteOwnOrder(currentUser.Id);
                        break;
                    case "0":
                        break;
                    default:
                        Console.WriteLine("Ogiltigt val.");
                        break;
                }

            } while (input != "0");
        }


        private void UserMenu()
        {
            string? input;
            do
            {
                Console.Clear();
                Console.WriteLine($"Inloggad som: {currentUser?.Username} (User)");
                Console.WriteLine("1. Produkter");
                Console.WriteLine("2. Ordrar");
                Console.WriteLine("3. Min profil");
                Console.WriteLine("0. Logga ut");
                Console.Write("Val: ");
                input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        ProductMenu();
                        break;
                    case "2":
                        OrderMenu();
                        break;
                    case "3":
                        MyProfileMenu();
                        break;
                    case "0":
                        Logout();
                        break;
                    default:
                        Console.WriteLine("Ogiltigt val.");
                        break;
                }

            } while (input != "0");
        }


        private void AdminMenu()
        {
            string? input;
            do
            {
                Console.Clear();
                Console.WriteLine($"Inloggad som: {currentUser?.Username} (Admin)");
                Console.WriteLine("1. Produkter");
                Console.WriteLine("2. Ordrar");
                Console.WriteLine("3. Användare");
                Console.WriteLine("4. Min profil");
                Console.WriteLine("0. Logga ut");
                Console.Write("Val: ");
                input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        ProductMenu(admin: true);
                        break;
                    case "2":
                        OrderMenu(admin: true);
                        break;
                    case "3":
                        UserAdminMenu();
                        break;
                    case "4":
                        MyProfileMenu();
                        break;
                    case "0":
                        Logout();
                        break;
                    default:
                        Console.WriteLine("Ogiltigt val.");
                        break;
                }

            } while (input != "0");
        }


        private void LoginMenu()
        {
            Console.Clear();
            Console.WriteLine("== Logga in ==");

            Console.Write("Användarnamn: ");
            var username = Console.ReadLine();
            Console.Write("Lösenord: ");
            var password = ReadPassword();
            var user = dbContext.Users.FirstOrDefault(u => u.Username == username);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                currentUser = user;
                Console.WriteLine("Inloggning lyckades!");
                Thread.Sleep(1500);
            }
            else
            {
                Console.WriteLine("Fel användarnamn eller lösenord.");
                Thread.Sleep(1500);
            }
        }


        private void UserAdminMenu()
        {
            string? input;
            do
            {
                Console.Clear();
                Console.WriteLine("== Användarhantering (Admin) ==");
                Console.WriteLine("1. Lista användare");
                Console.WriteLine("2. Skapa ny användare");
                Console.WriteLine("3. Uppdatera användare");
                Console.WriteLine("4. Min profil");
                Console.WriteLine("5. Ta bort användare");
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
                        UpdateUser();
                        break;
                    case "4":
                        MyProfileMenu();
                        break;
                    case "5":
                        DeleteUser();
                        break;
                    case "0":
                        break;
                    default:
                        Console.WriteLine("Ogiltigt val.");
                        break;
                }

            } while (input != "0");
        }


        private void MyProfileMenu()
        {
            string? input;
            do
            {
                Console.Clear();
                Console.WriteLine("== Min profil ==");
                Console.WriteLine($"Användarnamn: {currentUser.Username}");
                Console.WriteLine($"Roll: {currentUser.Role}");
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
                        InvalidOption();
                        break;
                }

            } while (input != "0");
        }

        private void CreateProduct()
        {
            if (currentUser.Role != "Admin")
            {
                Console.WriteLine("Du har inte behörighet att skapa produkt.");
                return;
            }

            Console.Clear();
            Console.WriteLine("== Skapa produkt ==");

            Console.Write("Namn: ");
            var name = Console.ReadLine();

            Console.Write("Beskrivning: ");
            var description = Console.ReadLine();

            Console.Write("Pris: ");
            if (!decimal.TryParse(Console.ReadLine(), out var price))
            {
                Console.WriteLine("Ogiltigt pris.");
                Pause();
                return;
            }

            var product = new Product
            {
                Name = name,
                Description = description,
                Price = price
            };

            _productService.CreateProduct(product);
            Console.WriteLine("Produkt skapad!");
            Pause();
        }

        private void UpdateProduct()
        {
            if (currentUser.Role != "Admin")
            {
                Console.WriteLine("Du har inte behörighet att uppdatera produkt.");
                return;
            }

            Console.Clear();
            Console.WriteLine("== Uppdatera produkt ==");
            _productService.ListProducts();

            Console.Write("Ange produkt-ID att uppdatera: ");
            if (!int.TryParse(Console.ReadLine(), out var id))
            {
                Console.WriteLine("Ogiltigt ID.");
                Pause();
                return;
            }

            var product = _productService.GetProductById(id);
            if (product == null)
            {
                Console.WriteLine("Produkt hittades inte.");
                Pause();
                return;
            }

            Console.Write($"Nytt namn ({product.Name}): ");
            var name = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(name))
                product.Name = name;

            Console.Write($"Ny beskrivning ({product.Description}): ");
            var description = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(description))
                product.Description = description;

            Console.Write($"Nytt pris ({product.Price:C}): ");
            var priceInput = Console.ReadLine();
            if (decimal.TryParse(priceInput, out var newPrice))
                product.Price = newPrice;

            _productService.UpdateProduct(product);
            Console.WriteLine("Produkt uppdaterad!");
            Pause();
        }

        private void DeleteProduct()
        {
            if (currentUser.Role != "Admin")
            {
                Console.WriteLine("Du har inte behörighet att ta bort produkt.");
                return;
            }

            Console.Clear();
            Console.WriteLine("== Ta bort produkt ==");
            _productService.ListProducts();

            Console.Write("Ange produkt-ID att ta bort: ");
            if (!int.TryParse(Console.ReadLine(), out var id))
            {
                Console.WriteLine("Ogiltigt ID.");
                Pause();
                return;
            }

            _productService.DeleteProduct(id);
            Console.WriteLine("Produkt borttagen.");
            Pause();
        }

        private void ListUsers()
        {
            Console.Clear();
            Console.WriteLine("== Alla användare ==");

            var users = _userService.GetAllUsers().OrderBy(u => u.Username);

            foreach (var user in users)
            {
                Console.WriteLine($"ID: {user.Id} | Användarnamn: {user.Username} | Roll: {user.Role}");
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


        private void UpdateUser()
        {
            if (currentUser.Role != "Admin")
            {
                Console.WriteLine("Du har inte behörighet att ändra andra användare.");
                Pause();
                return;
            }

            Console.Clear();
            Console.WriteLine("== Uppdatera användare ==");
            ListUsers();

            Console.Write("Ange ID på användare att uppdatera: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ogiltigt ID.");
                Pause();
                return;
            }

            var user = _userService.GetUserById(id);
            if (user == null)
            {
                Console.WriteLine("Användare hittades inte.");
                Pause();
                return;
            }

            Console.Write($"Nytt användarnamn ({user.Username}): ");
            var newUsername = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newUsername))
                user.Username = newUsername;

            Console.Write("Vill du ändra lösenord? (j/n): ");
            var changePassword = Console.ReadLine()?.ToLower() == "j";
            if (changePassword)
            {
                Console.Write("Nytt lösenord: ");
                var newPassword = ReadPassword();
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }

            Console.Write($"Ny roll ({user.Role}) [User/Admin]: ");
            var newRole = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newRole) && (newRole == "User" || newRole == "Admin"))
            {
                if (user.Id == currentUser.Id && newRole != "Admin")
                {
                    Console.WriteLine("Du kan inte ändra din egen roll till 'User'.");
                }
                else
                {
                    user.Role = newRole;
                }
            }

            _userService.UpdateUser(user);
            Console.WriteLine("Användare uppdaterad!");
            Pause();
        }

        private void DeleteUser()
        {
            if (currentUser.Role != "Admin")
            {
                Console.WriteLine("Du har inte behörighet att ta bort andra användare.");
                return;
            }

            Console.Clear();
            Console.WriteLine("== Ta bort användare ==");

            ListUsers();

            Console.Write("Ange ID på användare att ta bort: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ogiltigt ID.");
                Pause();
                return;
            }

            if (currentUser.Id == id)
            {
                Console.WriteLine("Du kan inte ta bort dig själv.");
                Pause();
                return;
            }

            var user = _userService.GetUserById(id);
            if (user == null)
            {
                Console.WriteLine("Användare hittades inte.");
                Pause();
                return;
            }

            Console.Write($"Är du säker på att du vill ta bort användaren '{user.Username}'? (j/n): ");
            var confirm = Console.ReadLine()?.ToLower();
            if (confirm != "j")
            {
                Console.WriteLine("Borttagning avbruten.");
                Pause();
                return;
            }

            _userService.DeleteUser(id);
            Console.WriteLine("Användare borttagen.");
            Pause();
        }

        private void ChangeUsername()
        {
            Console.Write("Nytt användarnamn: ");
            var newUsername = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(newUsername))
            {
                currentUser.Username = newUsername;
                _userService.UpdateUser(currentUser);
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
                currentUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                _userService.UpdateUser(currentUser);
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

        private void Logout()
        {
            currentUser = null;
            Console.WriteLine("Du är nu utloggad.");
            Pause();
        }
    }
}
