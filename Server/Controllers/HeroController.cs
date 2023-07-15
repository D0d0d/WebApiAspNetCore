using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Server.Library;
using Server.Library.Requests;
using Server.Services;
using System.ComponentModel.DataAnnotations;

namespace Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class HeroController : ControllerBase
    {
        private readonly IHeroService _playerService;
        private readonly GameDBContext _context;
        public HeroController(IHeroService playerService, GameDBContext context)
        {
            _playerService = playerService;
            _context = context;

            /*
            var user = new User()
            {
                Username = "Dodo",
                PasswordHash = "password",
                Salt = "asdfsgfdh",
            };

            _context.Add(user);
            _context.SaveChanges();
            */
        }
        /*
        [HttpGet]
        public Hero Get([FromQuery] int id, string name)
        {
            var Hero = new Hero() { id = id };
            return Hero;
        }
        */

        /*
        [HttpGet("{id}")]
        public Hero Get([FromRoute] int id)
        {
            var Player = new Hero() { Id = id };
            _playerService.DoSomething();
            return Player;
        }*/
        [HttpPost("{id}")]
        public IActionResult Edit([FromRoute] int id, [FromBody] CreateHeroRequest request)
        {
            var HeroIdAvailable = JsonConvert.DeserializeObject<List<int>>( User.FindFirst("heroes").Value);

            if (!HeroIdAvailable.Contains(id)) return Unauthorized();
            
            var Hero = _context.Heroes.First(h => h.Id == id);

            Hero.Name = request.Name;
            return Ok();
        }


        [HttpPost]
        public Hero Post(CreateHeroRequest request)
        {
            var userId = int.Parse (User.FindFirst("id").Value);

            var user = _context.Users.Include(u => u.Heroes).Where(u => u.Id == userId).First();

            var hero = new Hero() { Name = request.Name, User = user };

            _context.Add(hero);
            _context.SaveChanges();
            return hero;
        }
    }
}
