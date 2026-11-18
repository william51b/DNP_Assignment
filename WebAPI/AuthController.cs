using ApiContracts;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository userRepo;

    public AuthController(IUserRepository userRepo)
    {
        this.userRepo = userRepo;
    }

    // POST /auth/login
    [HttpPost("login")]
    public ActionResult<UserDto> Login([FromBody] LoginRequest request)
    {
        User? user = userRepo.GetManyAsync()
            .SingleOrDefault(u => u.Username.ToLower() == request.UserName.ToLower());

        if (user is null)
            return Unauthorized("Username or password is incorrect.");

        if (user.Password != request.Password)
            return Unauthorized("Username or password is incorrect.");

        return Ok(new UserDto { Id = user.Id, UserName = user.Username });
    }
}