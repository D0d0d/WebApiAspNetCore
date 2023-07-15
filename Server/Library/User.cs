namespace Server.Library
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }

        public List<Hero> Heroes { get; } = new List<Hero>();
    }
}
