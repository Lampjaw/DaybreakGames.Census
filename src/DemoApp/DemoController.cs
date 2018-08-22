using DaybreakGames.Census;
using DemoApp.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace DemoApp
{
    [Route("/")]
    public class DemoController : Controller
    {
        private readonly ICensusQueryFactory _censusFactory;

        public DemoController(ICensusQueryFactory censusFactory)
        {
            _censusFactory = censusFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("character")]
        public async Task<IActionResult> GetCharacter(string characterName)
        {
            var query = _censusFactory.Create("character");
            query.Where("name.first_lower").StartsWith(characterName.ToLower());
            query.SetLimit(10);

            // In this example GetListAsync is passing a model to bind the response items to
            var characterList = await query.GetListAsync<CensusCharacterModel>();

            ViewData["CharacterList"] = JToken.FromObject(characterList).ToString();

            return View("Index");
        }

        [HttpPost("outfit")]
        public async Task<IActionResult> GetOutfit(string outfitAlias)
        {
            var query = _censusFactory.Create("outfit");
            query.Where("alias").Equals(outfitAlias);

            // If no model is specified in the Get request then it's returned as a JToken
            var outfit = await query.GetAsync();

            ViewData["Outfit"] = JToken.FromObject(outfit).ToString();

            return View("Index");
        }
    }
}
