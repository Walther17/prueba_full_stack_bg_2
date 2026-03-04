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
        private static List<int> _deletedIds = new();

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
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Views/Users/html/Index.cshtml");
            return View();
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] UserDto model)
        {
            TrimStrings(model);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            HttpResponseMessage response = await _httpClient.GetAsync(_usersApiUrl);
            List<UserDto> apiUsers = await response.Content.ReadFromJsonAsync<List<UserDto>>() ?? new List<UserDto>();

            int apiCount = apiUsers.Count(u => !_deletedIds.Contains(u.Id));

            int localCount = _users.Count;

            model.Id = (apiCount + localCount) + 1;

            _users.Add(model);

            return Ok(model);
        }

        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] UserDto model)
        {
            TrimStrings(model);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Traer todos los usuarios de la API
            HttpResponseMessage response = await _httpClient.GetAsync(_usersApiUrl);
            List<UserDto> apiUsers = await response.Content.ReadFromJsonAsync<List<UserDto>>();
            if (apiUsers == null) apiUsers = new List<UserDto>();

            // Unir con los locales (_users)
            List<UserDto> allUsers = apiUsers
                .Where(u => !_deletedIds.Contains(u.Id)) // ignorar eliminados
                .Concat(_users)
                .ToList();

            // Buscar usuario
            UserDto user = allUsers.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            // Actualizar campos
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

            // Si era de la API, agregar a locales para poder actualizar
            if (!_users.Any(u => u.Id == user.Id))
                _users.Add(user);

            return Ok(user);
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            // Si existe en locales, lo quitamos
            _users.RemoveAll(u => u.Id == id);

            // Marcamos como eliminado (para los de API)
            if (!_deletedIds.Contains(id))
                _deletedIds.Add(id);

            return Ok(new { message = "Usuario eliminado" });
        }

        [HttpGet("GetUsers")]
        public async Task<ActionResult<PagedResult<UserDto>>> GetUsers(
            int page = 1,
            int pageSize = 10,
            string search = null)
        {
            // Obtener usuarios de API pública
            HttpResponseMessage response = await _httpClient.GetAsync(_usersApiUrl);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode);

            List<UserDto> apiUsers =
                await response.Content.ReadFromJsonAsync<List<UserDto>>();

            if (apiUsers == null)
                apiUsers = new List<UserDto>();

            // Unir con los locales (_users)
            List<UserDto> allUsers = apiUsers
                .Concat(_users)
                .ToList();

            allUsers = allUsers
            .Where(u => !_deletedIds.Contains(u.Id))
            .ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                allUsers = allUsers.Where(u =>
                    u.Username.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.Name.Firstname.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.Address.City.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            int total = allUsers.Count;

            List<UserDto> pagedUsers = allUsers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new PagedResult<UserDto>
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
