using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WalletApi.Data;
using WalletApi.DTOs;
using WalletApi.Extensions;
using WalletApi.Model;

namespace WalletApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly WalletDbContext _context;

        public UsuariosController(WalletDbContext context)
        {
            _context = context;
        }

        // POST: api/usuarios/registro - PÚBLICO
        [HttpPost("registro")]
        public async Task<ActionResult<Usuario>> Registro(UsuarioRegistroDTO dto)
        {
            // Verificar si el email ya existe
            if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { mensaje = "El email ya está registrado" });

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                PasswordHash = dto.Password // Idealmente hashear con BCrypt
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPerfil), new { mensaje = "Usuario registrado", usuarioId = usuario.Id });
        }

        // GET: api/usuarios/perfil - PROTEGIDO (el usuario ve su propio perfil)
        [HttpGet("perfil")]
        [Authorize]
        public async Task<ActionResult<Usuario>> GetPerfil()
        {
            var userId = User.GetUserId();
            var usuario = await _context.Usuarios.FindAsync(userId);

            if (usuario == null)
                return NotFound();

            // No devolver password
            return Ok(new
            {
                usuario.Id,
                usuario.Nombre,
                usuario.Email
            });
        }

        // PUT: api/usuarios/perfil - PROTEGIDO (el usuario edita su propio perfil)
        [HttpPut("perfil")]
        [Authorize]
        public async Task<IActionResult> ActualizarPerfil(UsuarioUpdateDTO dto)
        {
            var userId = User.GetUserId();
            var usuario = await _context.Usuarios.FindAsync(userId);

            if (usuario == null)
                return NotFound();

            usuario.Nombre = dto.Nombre;

            // Si quiere cambiar email, verificar que no exista
            if (dto.Email != usuario.Email)
            {
                if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
                    return BadRequest(new { mensaje = "El email ya está en uso" });
                usuario.Email = dto.Email;
            }

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Perfil actualizado" });
        }

        // DELETE: api/usuarios/perfil - PROTEGIDO (el usuario elimina su cuenta)
        [HttpDelete("perfil")]
        [Authorize]
        public async Task<IActionResult> EliminarCuenta()
        {
            var userId = User.GetUserId();
            var usuario = await _context.Usuarios.FindAsync(userId);

            if (usuario == null)
                return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Cuenta eliminada" });
        }
    }
}
