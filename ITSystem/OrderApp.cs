using ITSystem.Data.Contexts;
using ITSystem.Data.Entities;
using BCrypt.Net;

namespace ITSystem
{
    internal class OrderApp
    {
        private readonly OrderDbContext dbContext;
        private User? currentUser = null;

        public OrderApp(OrderDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        internal void Init()
        {
            dbContext.Database.EnsureCreated();

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

        private void UserMenu()
        {
            string? input;
            do
            {
                Console.Clear();
                Console.WriteLine($"Inloggad som: {currentUser.Username} (User)");
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
                        ProfileMenu();
                        break;
                }

            } while (input != "0");

            currentUser = null;
        }

        private void AdminMenu()
        {
            string? input;
            do
            {
                Console.Clear();
                Console.WriteLine($"Inloggad som: {currentUser.Username} (Admin)");
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
                        ProfileMenu();
                        break;
                }

            } while (input != "0");

            currentUser = null;
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

        private void ListProducts()
        {
            Console.Clear();
            Console.WriteLine("== Produkter ==");

            foreach (var product in dbContext.Products)
            {
                Console.WriteLine($"ID: {product.Id} | {product.Name} - {product.Description} ({product.Price:C})");
            }

            Pause();
        }

        private void CreateOrder()
        {
            Console.Clear();
            Console.WriteLine("== Skapa order ==");

            ListProducts();

            Console.Write("Ange produkt-ID: ");
            if (int.TryParse(Console.ReadLine(), out int productId))
            {
                var product = dbContext.Products.Find(productId);
                if (product != null)
                {
                    var order = new Order
                    {
                        ProductId = productId,
                        UserId = currentUser.Id,
                        OrderDate = DateTime.Now,
                        Status = "Pending"
                    };
                    dbContext.Orders.Add(order);
                    dbContext.SaveChanges();
                    Console.WriteLine("Order skapad!");
                }
                else
                {
                    Console.WriteLine("Produkt hittades inte.");
                }
            }
            else
            {
                Console.WriteLine("Ogiltigt ID.");
            }

            Pause();
        }

        private void ListOrders()
        {
            Console.Clear();
            Console.WriteLine("== Dina ordrar ==");

            var orders = dbContext.Orders
                .Where(o => o.UserId == currentUser.Id)
                .Select(o => new
                {
                    o.Id,
                    o.OrderDate,
                    o.Status,
                    ProductName = o.Product.Name
                }).ToList();

            foreach (var o in orders)
            {
                Console.WriteLine($"Order ID: {o.Id} | {o.ProductName} | {o.OrderDate} | Status: {o.Status}");
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
