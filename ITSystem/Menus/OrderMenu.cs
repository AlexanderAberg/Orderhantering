using ITSystem.Data.Entities;
using ITSystem.Services;

namespace ITSystem.Menus
{
    internal class OrderMenu : IMenu
    {
        private readonly User _currentUser;
        private readonly IOrderService _orderService;

        public OrderMenu(User currentUser, Data.Contexts.OrderDbContext dbContext, IOrderService orderService)
        {
            _currentUser = currentUser;
            _orderService = orderService;
        }

        public void Run()
        {
            string? input;
            bool isAdmin = _currentUser.Role == "Admin";

            do
            {
                Console.Clear();
                Console.WriteLine("== Ordermeny ==");
                Console.WriteLine("1. Lista ordrar");
                Console.WriteLine("2. Skapa order");

                if (isAdmin)
                    Console.WriteLine("3. Hantera alla ordrar");
                else
                    Console.WriteLine("3. Ändra/Ta bort egna ordrar");

                Console.WriteLine("0. Tillbaka");
                Console.Write("Val: ");
                input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        _orderService.ListOrdersByUser(_currentUser.Id);
                        Pause();
                        break;
                    case "2":
                        _orderService.CreateOrder(_currentUser.Id);
                        Pause();
                        break;
                    case "3":
                        if (isAdmin)
                            _orderService.ManageAllOrders(_currentUser.Id);
                        else
                            _orderService.ModifyOrDeleteOwnOrder(_currentUser.Id);
                        Pause();
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

        private void Pause()
        {
            Console.WriteLine("\nTryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }
    }
}
