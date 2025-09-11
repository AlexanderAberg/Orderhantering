namespace ITSystem.Data.Entities
{
    internal class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } = "User";
        public  ICollection<Order> Orders { get; set; }
    }
}