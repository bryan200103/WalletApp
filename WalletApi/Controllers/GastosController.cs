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
    [Authorize]
    public class GastosController : ControllerBase
    {
        private readonly WalletDbContext _context;

        public GastosController(WalletDbContext context)
        {
            _context = context;
        }

        // GET: api/Gastos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetGastos()
        {
            var userId = User.GetUserId();
            var gastos = await _context.Gastos
                .Include(g => g.Categoria)
                .Where(g => g.UsuarioId == userId)
                .OrderByDescending(g => g.Fecha)
                .Select(g => new
                {
                    g.Id,
                    g.Monto,
                    g.Descripcion,
                    g.Fecha,
                    g.EsRecurrente,
                    Categoria = g.Categoria != null ? new
                    {
                        g.Categoria.Id,
                        g.Categoria.Nombre,
                        g.Categoria.Icono,
                        g.Categoria.Color
                    } : null
                })
                .ToListAsync();

            return gastos;
        }

        // GET api/Gastos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetGasto(int id)
        {
            var userId = User.GetUserId();
            var gasto = await _context.Gastos
                .Include(g => g.Categoria)
                .Where(g => g.Id == id && g.UsuarioId == userId)
                .Select(g => new
                {
                    g.Id,
                    g.Monto,
                    g.Descripcion,
                    g.Fecha,
                    g.EsRecurrente,
                    Categoria = g.Categoria != null ? new
                    {
                        g.Categoria.Id,
                        g.Categoria.Nombre,
                        g.Categoria.Icono,
                        g.Categoria.Color
                    } : null
                })
                .FirstOrDefaultAsync();

            if (gasto == null)
                return NotFound(new { mensaje = "Gasto no encontrado" });

            return gasto;
        }

        // GET api/Gastos/mes?anio=2025&mes=1
        [HttpGet("mes")]
        public async Task<ActionResult<IEnumerable<object>>> GetGastosPorMes([FromQuery] int anio, [FromQuery] int mes)
        {
            var userId = User.GetUserId();
            var gastos = await _context.Gastos
                .Include(g => g.Categoria)
                .Where(g => g.UsuarioId == userId
                    && g.Fecha.Year == anio
                    && g.Fecha.Month == mes)
                .OrderByDescending(g => g.Fecha)
                .Select(g => new
                {
                    g.Id,
                    g.Monto,
                    g.Descripcion,
                    g.Fecha,
                    g.EsRecurrente,
                    Categoria = g.Categoria != null ? new
                    {
                        g.Categoria.Id,
                        g.Categoria.Nombre,
                        g.Categoria.Icono,
                        g.Categoria.Color
                    } : null
                })
                .ToListAsync();

            return gastos;
        }

        // GET api/Gastos/categoria/5
        [HttpGet("categoria/{categoriaId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetGastosPorCategoria(int categoriaId)
        {
            var userId = User.GetUserId();
            var gastos = await _context.Gastos
                .Include(g => g.Categoria)
                .Where(g => g.UsuarioId == userId && g.CategoriaId == categoriaId)
                .OrderByDescending(g => g.Fecha)
                .Select(g => new
                {
                    g.Id,
                    g.Monto,
                    g.Descripcion,
                    g.Fecha,
                    g.EsRecurrente,
                    Categoria = g.Categoria != null ? new
                    {
                        g.Categoria.Id,
                        g.Categoria.Nombre,
                        g.Categoria.Icono,
                        g.Categoria.Color
                    } : null
                })
                .ToListAsync();

            return gastos;
        }

        // POST api/Gastos
        [HttpPost]
        public async Task<ActionResult<object>> PostGasto(GastoDTO dto)
        {
            var userId = User.GetUserId();

            // Validar que la categoría existe y pertenece al usuario
            if (dto.CategoriaId.HasValue)
            {
                var categoriaExiste = await _context.Categorias
                    .AnyAsync(c => c.Id == dto.CategoriaId && c.UsuarioId == userId && c.Tipo == "Gasto");

                if (!categoriaExiste)
                    return BadRequest(new { mensaje = "Categoría no válida" });
            }

            var gasto = new Gasto
            {
                UsuarioId = userId,
                Descripcion = dto.Descripcion,
                Monto = dto.Monto,
                Fecha = dto.Fecha,
                CategoriaId = dto.CategoriaId
            };

            _context.Gastos.Add(gasto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGasto), new { id = gasto.Id }, new { mensaje = "Gasto creado", gasto.Id });
        }

        // PUT api/Gastos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGasto(int id, GastoDTO dto)
        {
            var userId = User.GetUserId();
            var gasto = await _context.Gastos
                .FirstOrDefaultAsync(g => g.Id == id && g.UsuarioId == userId);

            if (gasto == null)
                return NotFound(new { mensaje = "Gasto no encontrado" });

            // Validar categoría si se envía
            if (dto.CategoriaId.HasValue)
            {
                var categoriaExiste = await _context.Categorias
                    .AnyAsync(c => c.Id == dto.CategoriaId && c.UsuarioId == userId && c.Tipo == "Gasto");

                if (!categoriaExiste)
                    return BadRequest(new { mensaje = "Categoría no válida" });
            }

            gasto.Descripcion = dto.Descripcion;
            gasto.Monto = dto.Monto;
            gasto.Fecha = dto.Fecha;
            gasto.CategoriaId = dto.CategoriaId;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Gasto actualizado" });
        }

        // DELETE api/Gastos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGasto(int id)
        {
            var userId = User.GetUserId();
            var gasto = await _context.Gastos
                .FirstOrDefaultAsync(g => g.Id == id && g.UsuarioId == userId);

            if (gasto == null)
                return NotFound(new { mensaje = "Gasto no encontrado" });

            _context.Gastos.Remove(gasto);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Gasto eliminado correctamente" });
        }
    }
}
