using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WalletApi.Data;
using WalletApi.DTOs;
using WalletApi.Extensions;
using WalletApi.Model;

namespace WalletApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoriasController : ControllerBase
    {
        private readonly WalletDbContext _context;

        public CategoriasController(WalletDbContext context)
        {
            _context = context;
        }

        // GET: api/Categorias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCategorias()
        {
            var userId = User.GetUserId();
            var categorias = await _context.Categorias
                .Where(c => c.UsuarioId == userId)
                .OrderBy(c => c.Tipo)
                .ThenBy(c => c.Nombre)
                .Select(c => new
                {
                    c.Id,
                    c.Nombre,
                    c.Tipo,
                    c.Icono,
                    c.Color
                })
                .ToListAsync();

            return categorias;
        }

        // GET: api/Categorias/tipo/Gasto
        [HttpGet("tipo/{tipo}")]
        public async Task<ActionResult<IEnumerable<object>>> GetCategoriasPorTipo(string tipo)
        {
            var userId = User.GetUserId();
            var categorias = await _context.Categorias
                .Where(c => c.UsuarioId == userId && c.Tipo.ToLower() == tipo.ToLower())
                .OrderBy(c => c.Nombre)
                .Select(c => new
                {
                    c.Id,
                    c.Nombre,
                    c.Tipo,
                    c.Icono,
                    c.Color
                })
                .ToListAsync();

            return categorias;
        }

        // GET: api/Categorias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetCategoria(int id)
        {
            var userId = User.GetUserId();
            var categoria = await _context.Categorias
                .Where(c => c.Id == id && c.UsuarioId == userId)
                .Select(c => new
                {
                    c.Id,
                    c.Nombre,
                    c.Tipo,
                    c.Icono,
                    c.Color
                })
                .FirstOrDefaultAsync();

            if (categoria == null)
                return NotFound(new { mensaje = "Categoría no encontrada" });

            return categoria;
        }

        // POST: api/Categorias
        [HttpPost]
        public async Task<ActionResult<object>> PostCategoria(CategoriaDTO dto)
        {
            var userId = User.GetUserId();

            // Validar tipo
            if (dto.Tipo != "Gasto" && dto.Tipo != "Ingreso")
                return BadRequest(new { mensaje = "El tipo debe ser 'Gasto' o 'Ingreso'" });

            // Verificar si ya existe una categoría con ese nombre y tipo
            var existe = await _context.Categorias
                .AnyAsync(c => c.UsuarioId == userId
                    && c.Nombre.ToLower() == dto.Nombre.ToLower()
                    && c.Tipo == dto.Tipo);

            if (existe)
                return BadRequest(new { mensaje = "Ya existe una categoría con ese nombre" });

            var categoria = new Categoria
            {
                UsuarioId = userId,
                Nombre = dto.Nombre,
                Tipo = dto.Tipo,
                Icono = dto.Icono,
                Color = dto.Color
            };

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id },
                new { mensaje = "Categoría creada", categoria.Id });
        }

        // PUT: api/Categorias/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategoria(int id, CategoriaDTO dto)
        {
            var userId = User.GetUserId();
            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == userId);

            if (categoria == null)
                return NotFound(new { mensaje = "Categoría no encontrada" });

            // Validar tipo
            if (dto.Tipo != "Gasto" && dto.Tipo != "Ingreso")
                return BadRequest(new { mensaje = "El tipo debe ser 'Gasto' o 'Ingreso'" });

            categoria.Nombre = dto.Nombre;
            categoria.Tipo = dto.Tipo;
            categoria.Icono = dto.Icono;
            categoria.Color = dto.Color;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Categoría actualizada" });
        }

        // En tu CategoriasController.cs

        [HttpGet("{id}/puede-eliminar")]
        public async Task<ActionResult<object>> PuedeEliminar(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == userId);

            if (categoria == null)
                return NotFound(new { mensaje = "Categoría no encontrada" });

            // Contar usos en ingresos y gastos
            var usosIngresos = await _context.Ingresos
                .CountAsync(i => i.CategoriaId == id);

            var usosGastos = await _context.Gastos
                .CountAsync(g => g.CategoriaId == id);

            var totalUsos = usosIngresos + usosGastos;

            return Ok(new
            {
                puedeEliminar = totalUsos == 0,
                usosIngresos,
                usosGastos,
                totalUsos,
                mensaje = totalUsos > 0
                    ? $"Esta categoría está siendo usada en {totalUsos} registro(s)"
                    : "Se puede eliminar"
            });
        }




        // DELETE: api/Categorias/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteCategoria(int id)
        //{
        //    var userId = User.GetUserId();
        //    var categoria = await _context.Categorias
        //        .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == userId);

        //    if (categoria == null)
        //        return NotFound(new { mensaje = "Categoría no encontrada" });

        //    // Verificar si tiene gastos o ingresos asociados
        //    var tieneGastos = await _context.Gastos.AnyAsync(g => g.CategoriaId == id);

        //    if (tieneGastos)
        //        return BadRequest(new { mensaje = "No se puede eliminar. La categoría tiene registros asociados" });

        //    _context.Categorias.Remove(categoria);
        //    await _context.SaveChangesAsync();

        //    return Ok(new { mensaje = "Categoría eliminada" });
        //}




        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCategoria(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == userId);

            if (categoria == null)
                return NotFound(new { mensaje = "Categoría no encontrada" });

            // Verificar si está en uso
            var usosIngresos = await _context.Ingresos.CountAsync(i => i.CategoriaId == id);
            var usosGastos = await _context.Gastos.CountAsync(g => g.CategoriaId == id);
            var totalUsos = usosIngresos + usosGastos;

            if (totalUsos > 0)
            {
                return BadRequest(new
                {
                    mensaje = $"No se puede eliminar. La categoría está siendo usada en {usosIngresos} ingreso(s) y {usosGastos} gasto(s).",
                    usosIngresos,
                    usosGastos,
                    totalUsos
                });
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Categoría eliminada correctamente" });


        }





    }
}
