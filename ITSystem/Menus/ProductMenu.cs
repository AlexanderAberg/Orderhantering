using ITSystem.Data.Contexts;
using ITSystem.Data.Entities;
using ITSystem.Menus;
using ITSystem.Services;

internal class ProductMenu : IMenu
{
    private readonly User currentUser;
    private readonly OrderDbContext dbContext;
    private readonly IProductService productService;
    private readonly bool isAdmin;

    public ProductMenu(User currentUser, OrderDbContext dbContext, IProductService productService)
    {
        this.currentUser = currentUser;
        this.dbContext = dbContext;
        this.productService = productService;
        this.isAdmin = currentUser.Role == "Admin";
    }

    public void Run()
    {
        string? input;
        do
        {
            Console.Clear();
            Console.WriteLine("== Produktmeny ==");
            Console.WriteLine("1. Lista produkter");

            if (isAdmin)
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
                    productService.ListProducts();
                    Pause();
                    break;
                case "2":
                    if (isAdmin)
                        CreateProduct();
                    else
                        Deny();
                    break;
                case "3":
                    if (isAdmin)
                        UpdateProduct();
                    else
                        Deny();
                    break;
                case "4":
                    if (isAdmin)
                        DeleteProduct();
                    else
                        Deny();
                    break;
                case "0":
                    break;
                default:
                    Console.WriteLine("Ogiltigt val.");
                    break;
            }

        } while (input != "0");
    }

    private void Deny()
    {
        Console.WriteLine("Endast administratörer har tillgång till denna funktion.");
        Pause();
    }

    private void Pause()
    {
        Console.WriteLine("Tryck på valfri tangent för att fortsätta...");
        Console.ReadKey();
    }

    private void CreateProduct()
        {
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
                Console.WriteLine("Tryck på valfri tangent för att fortsätta...");
                Console.ReadKey();
                return;
            }

            var product = new Product
            {
                Name = name,
                Description = description,
                Price = price
            };

            productService.CreateProduct(product);
            Console.WriteLine("Produkt skapad!");
            Console.WriteLine("Tryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }

        private void UpdateProduct()
        {
            Console.Clear();
            Console.WriteLine("== Uppdatera produkt ==");
            productService.ListProducts();

            Console.Write("Ange produkt-ID att uppdatera: ");
            if (!int.TryParse(Console.ReadLine(), out var id))
            {
                Console.WriteLine("Ogiltigt ID.");
                Console.WriteLine("Tryck på valfri tangent för att fortsätta...");
                Console.ReadKey();
                return;
            }

            var product = productService.GetProductById(id);
            if (product == null)
            {
                Console.WriteLine("Produkt hittades inte.");
                Console.WriteLine("Tryck på valfri tangent för att fortsätta...");
                Console.ReadKey();
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

            productService.UpdateProduct(product);
            Console.WriteLine("Produkt uppdaterad!");
            Console.WriteLine("Tryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }

        private void DeleteProduct()
        {
            Console.Clear();
            Console.WriteLine("== Ta bort produkt ==");
            productService.ListProducts();

            Console.Write("Ange produkt-ID att ta bort: ");
            if (!int.TryParse(Console.ReadLine(), out var id))
            {
                Console.WriteLine("Ogiltigt ID.");
                Console.WriteLine("Tryck på valfri tangent för att fortsätta...");
                Console.ReadKey();
                return;
            }

            productService.DeleteProduct(id);
            Console.WriteLine("Produkt borttagen.");
            Console.WriteLine("Tryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }
    }
