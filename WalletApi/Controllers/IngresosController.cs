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
    public class IngresosController : ControllerBase
    {
        private readonly WalletDbContext _context;

        public IngresosController(WalletDbContext context)
        {
            _context = context;
        }

        // GET: api/ingresos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetIngresos()
        {
            var userId = User.GetUserId();
            var ingresos = await _context.Ingresos
                .Include(i => i.Categoria)
                .Where(i => i.UsuarioId == userId)
                .OrderByDescending(i => i.Fecha)
                .Select(i => new
                {
                    i.Id,
                    i.Monto,
                    i.Descripcion,
                    i.Fecha,
                    i.EsRecurrente,
                    Categoria = i.Categoria == null ? null : new
                    {
                        i.Categoria.Id,
                        i.Categoria.Nombre,
                        i.Categoria.Icono,
                        i.Categoria.Color
                    }
                })
                .ToListAsync();

            return ingresos;
        }

        // GET: api/ingresos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetIngreso(int id)
        {
            var userId = User.GetUserId();
            var ingreso = await _context.Ingresos
                .Include(i => i.Categoria)
                .Where(i => i.Id == id && i.UsuarioId == userId)
                .Select(i => new
                {
                    i.Id,
                    i.Monto,
                    i.Descripcion,
                    i.Fecha,
                    i.EsRecurrente,
                    Categoria = i.Categoria == null ? null : new
                    {
                        i.Categoria.Id,
                        i.Categoria.Nombre,
                        i.Categoria.Icono,
                        i.Categoria.Color
                    }
                })
                .FirstOrDefaultAsync();

            if (ingreso == null)
                return NotFound(new { mensaje = "Ingreso no encontrado" });

            return ingreso;
        }

        // GET: api/ingresos/mes?anio=2025&mes=1
        [HttpGet("mes")]
        public async Task<ActionResult<IEnumerable<object>>> GetIngresosPorMes(
            [FromQuery] int anio, [FromQuery] int mes)
        {
            var userId = User.GetUserId();
            var ingresos = await _context.Ingresos
                .Include(i => i.Categoria)
                .Where(i => i.UsuarioId == userId
                    && i.Fecha.Year == anio
                    && i.Fecha.Month == mes)
                .OrderByDescending(i => i.Fecha)
                .Select(i => new
                {
                    i.Id,
                    i.Monto,
                    i.Descripcion,
                    i.Fecha,
                    i.EsRecurrente,
                    Categoria = i.Categoria == null ? null : new
                    {
                        i.Categoria.Id,
                        i.Categoria.Nombre,
                        i.Categoria.Icono,
                        i.Categoria.Color
                    }
                })
                .ToListAsync();

            return ingresos;
        }

        // POST: api/ingresos
        [HttpPost]
        public async Task<ActionResult<object>> PostIngreso(IngresoDTO dto)
        {
            var userId = User.GetUserId();

            // Validar categoría si se proporciona
            if (dto.CategoriaId.HasValue)
            {
                var categoria = await _context.Categorias
                    .FirstOrDefaultAsync(c => c.Id == dto.CategoriaId && c.UsuarioId == userId);

                if (categoria == null)
                    return BadRequest(new { mensaje = "Categoría no encontrada" });

                if (categoria.Tipo != "Ingreso")
                    return BadRequest(new { mensaje = "La categoría debe ser de tipo 'Ingreso'" });
            }

            var ingreso = new Ingreso
            {
                UsuarioId = userId,
                Descripcion = dto.Descripcion,
                Monto = dto.Monto,
                Fecha = dto.Fecha ?? DateTime.Now,
                CategoriaId = dto.CategoriaId,
                EsRecurrente = dto.EsRecurrente
            };

            _context.Ingresos.Add(ingreso);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetIngreso), new { id = ingreso.Id },
                new { mensaje = "Ingreso creado", ingreso.Id });
        }

        // PUT: api/ingresos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIngreso(int id, IngresoDTO dto)
        {
            var userId = User.GetUserId();
            var ingreso = await _context.Ingresos
                .FirstOrDefaultAsync(i => i.Id == id && i.UsuarioId == userId);

            if (ingreso == null)
                return NotFound(new { mensaje = "Ingreso no encontrado" });

            // Validar categoría si se proporciona
            if (dto.CategoriaId.HasValue)
            {
                var categoria = await _context.Categorias
                    .FirstOrDefaultAsync(c => c.Id == dto.CategoriaId && c.UsuarioId == userId);

                if (categoria == null)
                    return BadRequest(new { mensaje = "Categoría no encontrada" });

                if (categoria.Tipo != "Ingreso")
                    return BadRequest(new { mensaje = "La categoría debe ser de tipo 'Ingreso'" });
            }

            ingreso.Descripcion = dto.Descripcion;
            ingreso.Monto = dto.Monto;
            ingreso.Fecha = dto.Fecha ?? ingreso.Fecha;
            ingreso.CategoriaId = dto.CategoriaId;
            ingreso.EsRecurrente = dto.EsRecurrente;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Ingreso actualizado" });
        }

        // DELETE: api/ingresos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIngreso(int id)
        {
            var userId = User.GetUserId();
            var ingreso = await _context.Ingresos
                .FirstOrDefaultAsync(i => i.Id == id && i.UsuarioId == userId);

            if (ingreso == null)
                return NotFound(new { mensaje = "Ingreso no encontrado" });

            _context.Ingresos.Remove(ingreso);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Ingreso eliminado correctamente" });
        }
    }
}
