using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using WebApplication1.Models;
using System.Text.Json;
namespace WebApplication1.Controllers
{

    [Route("Users")]
    public class UsersController : Controller
    {

        private readonly HttpClient _httpClient;
        public readonly string _usersApiUrl;
        private static List<UserDto> _users = new List<UserDto>();
        private static int _nextId = 1;

        public UsersController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _usersApiUrl = configuration["MyApiSettings:UsersUrl"];
        }


        private void TrimStrings(UserDto model)
        {
            model.Email = model.Email?.Trim();
            model.Username = model.Username?.Trim();
            model.Password = model.Password?.Trim();
            model.Phone = model.Phone?.Trim();

            model.Name.Firstname = model.Name.Firstname?.Trim();
            model.Name.Lastname = model.Name.Lastname?.Trim();

            model.Address.City = model.Address.City?.Trim();
            model.Address.Street = model.Address.Street?.Trim();
            model.Address.Zipcode = model.Address.Zipcode?.Trim();
        }

        // El IActionResult es una interfaz puede retornar (una vista, un JSON, un error 404, un bad request, etc.).
        public IActionResult Index()
        {
            return View("~/Views/Users/html/Index.cshtml");
            return View();
        }

        [HttpPost("Create")]
        public IActionResult Create([FromBody] UserDto model)
        {
            TrimStrings(model);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.Id = _nextId++;
            _users.Add(model);

            return Ok(model);
        }

        // ===============================
        // EDITAR
        // ===============================
        [HttpPut("Edit/{id}")]
        public IActionResult Edit(int id, [FromBody] UserDto model)
        {
            TrimStrings(model);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            UserDto user = _users.FirstOrDefault(u => u.Id == id);

            if (user == null)
                return NotFound();

            // ID NO se modifica
            user.Email = model.Email;
            user.Username = model.Username;
            user.Password = model.Password;
            user.Phone = model.Phone;

            user.Name.Firstname = model.Name.Firstname;
            user.Name.Lastname = model.Name.Lastname;

            user.Address.City = model.Address.City;
            user.Address.Street = model.Address.Street;
            user.Address.Number = model.Address.Number;
            user.Address.Zipcode = model.Address.Zipcode;

            return Ok(user);
        }

        // ===============================
        // ELIMINAR
        // ===============================
        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            UserDto user = _users.FirstOrDefault(u => u.Id == id);

            if (user == null)
                return NotFound();

            _users.Remove(user);

            return Ok(new { message = "Usuario eliminado" });
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<UserDto>>> GetUsers(
        int page = 1,
        int pageSize = 10,
        string search = null)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(_usersApiUrl);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode);

            List<UserDto> users = await response.Content.ReadFromJsonAsync<List<UserDto>>();

            if (users == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users.Where(u =>
                    u.Username.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.Name.Firstname.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.Address.City.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            int total = users.Count;

            List<UserDto> pagedUsers = users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            PagedResult<UserDto> result = new PagedResult<UserDto>
            {
                Data = pagedUsers,
                TotalRecords = total,
                Page = page,
                PageSize = pageSize
            };

            return Ok(result);
        }


        // ActionResult es una clase base que implementa IActionResult, se usa mucho en Web APIs. Te permite devolver tanto un tipo específico (como una clase Userso) como un resultado HTTP (como NotFound).
        // Task se usa cuando el método es asíncrono (async).
        //[HttpGet]
        //public async Task<ActionResult<Users>> Get()
        //{
        //    Users Users = await _service.Get();

        //    if (Users == null)
        //        return NotFound();

        //    return Users; // No necesitas Ok()
        //}
    }
}
