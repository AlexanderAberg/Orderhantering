using ITSystem.Data.Contexts;
using ITSystem.Data.Entities;
using ITSystem.Services;

namespace ITSystem.Menus
{
    internal class AdminMenu : IMenu
    {
        private readonly User currentUser;
        private readonly OrderDbContext dbContext;
        private readonly IOrderService orderService;
        private readonly IUserService userService;
        private readonly IProductService productService;

        public AdminMenu(
            User currentUser,
            OrderDbContext dbContext,
            IOrderService orderService,
            IUserService userService,
            IProductService productService)
        {
            this.currentUser = currentUser;
            this.dbContext = dbContext;
            this.orderService = orderService;
            this.userService = userService;
            this.productService = productService;
        }

        public void Run()
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
                        var productMenu = new ProductMenu(currentUser, dbContext, productService);
                        productMenu.Run();
                        break;
                    case "2":
                        var orderMenu = new OrderMenu(currentUser, dbContext, orderService);
                        orderMenu.Run(  );
                        break;
                    case "3":
                        var userAdminMenu = new UserAdminMenu(currentUser, dbContext, userService);
                        userAdminMenu.Run();
                        break;
                    case "4":
                        var profileMenu = new ProfileMenu(currentUser, userService);
                        profileMenu.Run();
                        break;
                    case "0":
                        Console.WriteLine("Du är nu utloggad.");
                        break;
                    default:
                        Console.WriteLine("Ogiltigt val.");
                        Console.WriteLine("Tryck på valfri tangent för att fortsätta...");
                        Console.ReadKey();
                        break;
                }

            } while (input != "0");
        }
    }
}
