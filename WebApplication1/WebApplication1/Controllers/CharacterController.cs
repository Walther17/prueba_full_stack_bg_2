using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Route("Character")]
    public class CharacterController : Controller
    {
        public readonly HttpClient _httpClient;
        public readonly string _url;

        public CharacterController()
        {
            
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
