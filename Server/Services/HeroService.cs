namespace Server.Services
{
    public interface IHeroService
    {
        public void DoSomething();

    }
    public class HeroService : IHeroService
    {
        public void DoSomething()
        {
            System.Diagnostics.Debug.WriteLine("hey!");
        }
    }
    public class MockPlayerService : IHeroService
    {
        public void DoSomething()
        {
            System.Diagnostics.Debug.WriteLine("hey!");
        }
    }
}
