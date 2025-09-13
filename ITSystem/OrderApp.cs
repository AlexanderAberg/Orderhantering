using ITSystem.Data;
using ITSystem.Data.Contexts;
using ITSystem.Data.Entities;
using ITSystem.Menus;
using ITSystem.Services;
using Microsoft.EntityFrameworkCore;

namespace ITSystem
{
    internal class OrderApp
    {
        private readonly OrderDbContext dbContext;
        private readonly IOrderService orderService;
        private readonly IUserService userService;
        private readonly IProductService productService;

        public OrderApp(
            OrderDbContext dbContext,
            IOrderService orderService,
            IUserService userService,
            IProductService productService)
        {
            this.dbContext = dbContext;
            this.orderService = orderService;
            this.userService = userService;
            this.productService = productService;
        }

        internal void Init()
        {
            dbContext.Database.EnsureCreated();
            DataSeeder.SeedIfEmpty(dbContext);
        }

        internal void RunMenu()
        {
            var loginMenu = new LoginMenu(dbContext);
            var currentUser = loginMenu.Login();

            if (currentUser == null)
                return;

            IMenu menu = currentUser.Role switch
            {
                "Admin" => new AdminMenu(currentUser, dbContext, orderService, userService, productService),
                _ => new UserMenu(currentUser, dbContext, orderService, userService, productService)
            };

            menu.Run();
        }
    }
}
