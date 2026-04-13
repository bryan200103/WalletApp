    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using WalletApi.Data;
    using WalletApi.Services;

    // For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

    namespace WalletApi.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class AuthController : ControllerBase
        {
            private readonly WalletDbContext _context;
            private readonly JwtService _jwtService;
            public AuthController(WalletDbContext context, JwtService jwtService)
            {
                _context = context;
                _jwtService = jwtService;
            }



            // POST api/<AuthController>
            [HttpPost("login")]
            public async Task<IActionResult> Login([FromBody] LoginDto dto)
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (usuario == null || usuario.PasswordHash != dto.Contraseña)
                    return Unauthorized("Credenciales inválidas");

                var token = _jwtService.GenerarToken(usuario);

                return Ok(new
                {
                    token,
                    usuario = new
                    {
                        usuario.Id,
                        usuario.Nombre,
                        usuario.Email
                    }
                });
            }
        }

        public class LoginDto
        {
            public string Email { get; set; } = string.Empty;
            public string Contraseña { get; set; } = string.Empty;
        }



    }

