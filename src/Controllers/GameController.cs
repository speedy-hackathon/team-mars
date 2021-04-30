using covidSim.Services;
using Microsoft.AspNetCore.Mvc;

namespace covidSim.Controllers
{
    [Route("api/state")]
    public class GameController : Controller
    {
        [HttpGet]
        public IActionResult State()
        {
            var game = Game.Instance;
            game = game.GetNextState();
            return Ok(game);
        }

        [HttpPost]
        public IActionResult Restart()
        {
            var game = Game.Restart;
            return Ok(game);
        }
    }
}
