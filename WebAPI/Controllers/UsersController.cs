using ApiContracts;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository userRepo;

    public UsersController(IUserRepository userRepo)
    {
        this.userRepo = userRepo;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> AddUser([FromBody] CreateUserDto request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Username and password are required.");

        if (UserNameTaken(request.UserName))
            return Conflict($"Username '{request.UserName}' is already taken.");

        User created = await userRepo.AddAsync(new User
        {
            Username = request.UserName,
            Password = request.Password
        });

        UserDto dto = ToDto(created);
        return Created($"/users/{dto.Id}", dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> UpdateUser([FromRoute] int id, [FromBody] UpdateUserDto request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Username and password are required.");

        User existing = await userRepo.GetSingleAsync(id);

        if (UserNameTaken(request.UserName, excludeId: id))
            return Conflict($"Username '{request.UserName}' is already taken.");

        existing.Username = request.UserName;
        existing.Password = request.Password;
        await userRepo.UpdateAsync(existing);

        return Ok(ToDto(existing));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetUser([FromRoute] int id)
    {
        User user = await userRepo.GetSingleAsync(id);
        return Ok(ToDto(user));
    }

    // GET /users?userNameContains=abc
    [HttpGet]
    public ActionResult<IEnumerable<UserDto>> GetUsers([FromQuery] string? userNameContains)
    {
        IQueryable<User> users = userRepo.GetManyAsync();

        if (!string.IsNullOrWhiteSpace(userNameContains))
            users = users.Where(u => u.Username.ToLower().Contains(userNameContains.ToLower()));

        return Ok(users.Select(u => new UserDto { Id = u.Id, UserName = u.Username }).ToList());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser([FromRoute] int id)
    {
        await userRepo.DeleteAsync(id);
        return NoContent();
    }

    private bool UserNameTaken(string userName, int? excludeId = null) =>
        userRepo.GetManyAsync().Any(u =>
            u.Username.ToLower() == userName.ToLower() && u.Id != excludeId);

    private static UserDto ToDto(User u) => new() { Id = u.Id, UserName = u.Username };
}