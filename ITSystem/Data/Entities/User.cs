namespace ITSystem.Data.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = "User";

        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public ICollection<Order> EditedOrders { get; set; } = new List<Order>();
    }
}
