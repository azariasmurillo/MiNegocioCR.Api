using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.API.Helpers;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Auth;
using MiNegocioCR.Api.Application.Interfaces.Repositories;

namespace MiNegocioCR.Api.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IFirebaseAuthService _firebaseAuth;
        private readonly IUserRepository _userRepository;

        public AuthController(
            IFirebaseAuthService firebaseAuth,
            IUserRepository userRepository)
        {
            _firebaseAuth = firebaseAuth;
            _userRepository = userRepository;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] FirebaseLoginRequestDto request)
        {
            var uid = await _firebaseAuth.VerifyTokenAsync(request.Token);

            if (uid == null)
                return Unauthorized();

            var user = await _userRepository.GetByFirebaseUidAsync(uid);

            if (user == null)
            {
                user = await _userRepository.CreateFromFirebaseAsync(uid);
            }

            return Ok(user);
        }

        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var uid = AuthHelper.GetFirebaseUid(HttpContext);

            if (uid == null)
                return Unauthorized();

            return Ok(new { firebaseUid = uid });
        }
    }
}
