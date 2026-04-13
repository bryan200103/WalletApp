using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WalletApi.Data;
using WalletApi.DTOs;
using WalletApi.Extensions;
using WalletApi.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WalletApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MetasController : ControllerBase
    {
        private readonly WalletDbContext _context;

        public MetasController(WalletDbContext context)
        {
            _context = context;
        }
        // GET: api/<MetasController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MetaResponseDTO>>> GetMetas(
            [FromQuery] bool? completada = null,
            [FromQuery] string? ordenarPor = "fechaCreacion",
            [FromQuery] bool descendente = true)
        {
            var usuarioId = User.GetUserId();

            var query = _context.Metas
                .Include(m => m.Aportes)
                .Where(m => m.UsuarioId == usuarioId);

            // Filtro por estado
            if (completada.HasValue)
            {
                query = query.Where(m => m.Completada == completada.Value);
            }

            // Ordenamiento
            query = ordenarPor?.ToLower() switch
            {
                "nombre" => descendente ? query.OrderByDescending(m => m.Nombre) : query.OrderBy(m => m.Nombre),
                "montoobjetivo" => descendente ? query.OrderByDescending(m => m.MontoObjetivo) : query.OrderBy(m => m.MontoObjetivo),
                "fechalimite" => descendente ? query.OrderByDescending(m => m.FechaLimite) : query.OrderBy(m => m.FechaLimite),
                "progreso" => descendente ? query.OrderByDescending(m => m.MontoActual / m.MontoObjetivo) : query.OrderBy(m => m.MontoActual / m.MontoObjetivo),
                _ => descendente ? query.OrderByDescending(m => m.FechaCreacion) : query.OrderBy(m => m.FechaCreacion)
            };

            var metas = await query.ToListAsync();

            return Ok(metas.Select(MapToResponseDTO));
        }

        // GET api/<MetasController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MetaResponseDTO>> GetMeta(int id)
        {
            var usuarioId = User.GetUserId();

            var meta = await _context.Metas
                .Include(m => m.Aportes!.OrderByDescending(a => a.FechaAporte))
                .FirstOrDefaultAsync(m => m.Id == id && m.UsuarioId == usuarioId);

            if (meta == null)
            {
                return NotFound(new { message = "Meta no encontrada" });
            }

            return Ok(MapToResponseDTO(meta));
        }
        [HttpGet("resumen")]
        public async Task<ActionResult<MetasResumenDTO>> GetResumen()
        {
            var usuarioId = User.GetUserId();

            var metas = await _context.Metas
                .Where(m => m.UsuarioId == usuarioId)
                .ToListAsync();

            var ahora = DateTime.Now;

            var resumen = new MetasResumenDTO
            {
                TotalMetas = metas.Count,
                MetasCompletadas = metas.Count(m => m.Completada),
                MetasEnProgreso = metas.Count(m => !m.Completada && m.FechaLimite >= ahora),
                MetasVencidas = metas.Count(m => !m.Completada && m.FechaLimite < ahora),
                TotalAhorrado = metas.Sum(m => m.MontoActual),
                TotalObjetivo = metas.Sum(m => m.MontoObjetivo),
                PorcentajeGeneral = metas.Any()
                    ? Math.Round((metas.Sum(m => m.MontoActual) / metas.Sum(m => m.MontoObjetivo)) * 100, 2)
                    : 0
            };

            return Ok(resumen);
        }

        [HttpGet("proximas-vencer")]
        public async Task<ActionResult<IEnumerable<MetaResponseDTO>>> GetProximasVencer([FromQuery] int dias = 30)
        {
            var usuarioId = User.GetUserId();
            var fechaLimite = DateTime.Now.AddDays(dias);

            var metas = await _context.Metas
                .Include(m => m.Aportes)
                .Where(m => m.UsuarioId == usuarioId &&
                           !m.Completada &&
                           m.FechaLimite <= fechaLimite &&
                           m.FechaLimite >= DateTime.Now)
                .OrderBy(m => m.FechaLimite)
                .ToListAsync();

            return Ok(metas.Select(MapToResponseDTO));
        }


        [HttpPost]
        public async Task<ActionResult<MetaResponseDTO>> CreateMeta([FromBody] MetaDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuarioId = User.GetUserId();

            // Validar fecha límite
            if (dto.FechaLimite <= DateTime.Now)
            {
                return BadRequest(new { message = "La fecha límite debe ser posterior a la fecha actual" });
            }

            var meta = new Meta
            {
                Nombre = dto.Nombre.Trim(),
                Descripcion = dto.Descripcion?.Trim(),
                MontoObjetivo = dto.MontoObjetivo,
                MontoActual = 0,
                FechaLimite = dto.FechaLimite,
                FechaCreacion = DateTime.Now,
                Completada = false,
                Icono = dto.Icono,
                Color = dto.Color,
                UsuarioId = usuarioId
            };

            _context.Metas.Add(meta);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMeta), new { id = meta.Id }, MapToResponseDTO(meta));
        }



        [HttpPut("{id}")]
        public async Task<ActionResult<MetaResponseDTO>> UpdateMeta(int id, [FromBody] MetaDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuarioId = User.GetUserId();

            var meta = await _context.Metas.FindAsync(id);

            if (meta == null || meta.UsuarioId != usuarioId)
            {
                return NotFound(new { message = "Meta no encontrada" });
            }

            // Validar que el nuevo monto objetivo no sea menor al monto actual
            if (dto.MontoObjetivo < meta.MontoActual)
            {
                return BadRequest(new { message = $"El monto objetivo no puede ser menor al monto actual ahorrado ({meta.MontoActual:C})" });
            }

            meta.Nombre = dto.Nombre.Trim();
            meta.Descripcion = dto.Descripcion?.Trim();
            meta.MontoObjetivo = dto.MontoObjetivo;
            meta.FechaLimite = dto.FechaLimite;
            meta.Icono = dto.Icono;
            meta.Color = dto.Color;

            // Verificar si se completó la meta con el nuevo objetivo
            meta.Completada = meta.MontoActual >= meta.MontoObjetivo;

            await _context.SaveChangesAsync();

            return Ok(MapToResponseDTO(meta));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeta(int id)
        {
            var usuarioId = User.GetUserId();

            var meta = await _context.Metas.FindAsync(id);

            if (meta == null || meta.UsuarioId != usuarioId)
            {
                return NotFound(new { message = "Meta no encontrada" });
            }

            _context.Metas.Remove(meta);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Meta eliminada correctamente" });
        }

        [HttpPost("{id}/aportar")]
        public async Task<ActionResult<MetaResponseDTO>> AportarMeta(int id, [FromBody] AporteMetaDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuarioId = User.GetUserId();

            var meta = await _context.Metas
                .Include(m => m.Aportes)
                .FirstOrDefaultAsync(m => m.Id == id && m.UsuarioId == usuarioId);

            if (meta == null)
            {
                return NotFound(new { message = "Meta no encontrada" });
            }

            if (meta.Completada)
            {
                return BadRequest(new { message = "Esta meta ya está completada" });
            }

            // Validar que el aporte no exceda el monto restante
            var montoRestante = meta.MontoObjetivo - meta.MontoActual;
            if (dto.Monto > montoRestante)
            {
                return BadRequest(new
                {
                    message = $"El aporte excede el monto restante. Máximo permitido: {montoRestante:C}",
                    montoMaximo = montoRestante
                });
            }

            // Crear el aporte
            var aporte = new AporteMeta
            {
                Monto = dto.Monto,
                Nota = dto.Nota?.Trim(),
                FechaAporte = DateTime.Now,
                MetaId = id,
                UsuarioId = usuarioId
            };

            _context.AportesMetas.Add(aporte);

            // Actualizar monto actual de la meta
            meta.MontoActual += dto.Monto;

            // Verificar si se completó la meta
            if (meta.MontoActual >= meta.MontoObjetivo)
            {
                meta.Completada = true;
            }

            await _context.SaveChangesAsync();

            // Recargar con aportes
            await _context.Entry(meta).Collection(m => m.Aportes!).LoadAsync();

            var response = MapToResponseDTO(meta);
            response.Aportes = meta.Aportes?
                .OrderByDescending(a => a.FechaAporte)
                .Select(a => new AporteMetaResponseDTO
                {
                    Id = a.Id,
                    Monto = a.Monto,
                    FechaAporte = a.FechaAporte,
                    Nota = a.Nota
                }).ToList();

            return Ok(new
            {
                message = meta.Completada ? "¡Felicidades! Has completado tu meta" : "Aporte registrado exitosamente",
                meta = response
            });
        }


        // DELETE: api/Metas/5/aportes/3
        [HttpDelete("{metaId}/aportes/{aporteId}")]
        public async Task<IActionResult> EliminarAporte(int metaId, int aporteId)
        {
            var usuarioId = User.GetUserId();

            var meta = await _context.Metas.FindAsync(metaId);
            if (meta == null || meta.UsuarioId != usuarioId)
            {
                return NotFound(new { message = "Meta no encontrada" });
            }

            var aporte = await _context.AportesMetas
                .FirstOrDefaultAsync(a => a.Id == aporteId && a.MetaId == metaId);

            if (aporte == null)
            {
                return NotFound(new { message = "Aporte no encontrado" });
            }

            // Restar el monto del aporte
            meta.MontoActual -= aporte.Monto;
            if (meta.MontoActual < 0) meta.MontoActual = 0;
            meta.Completada = meta.MontoActual >= meta.MontoObjetivo;

            _context.AportesMetas.Remove(aporte);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Aporte eliminado correctamente" });
        }

        [HttpGet("{id}/aportes")]
        public async Task<ActionResult<IEnumerable<AporteMetaResponseDTO>>> GetAportes(int id)
        {
            var usuarioId = User.GetUserId();

            var meta = await _context.Metas
                .FirstOrDefaultAsync(m => m.Id == id && m.UsuarioId == usuarioId);

            if (meta == null)
            {
                return NotFound(new { message = "Meta no encontrada" });
            }

            var aportes = await _context.AportesMetas
                .Where(a => a.MetaId == id)
                .OrderByDescending(a => a.FechaAporte)
                .Select(a => new AporteMetaResponseDTO
                {
                    Id = a.Id,
                    Monto = a.Monto,
                    FechaAporte = a.FechaAporte,
                    Nota = a.Nota
                })
                .ToListAsync();

            return Ok(aportes);
        }




        private static MetaResponseDTO MapToResponseDTO(Meta meta)
        {
            var ahora = DateTime.Now;
            string estado;

            if (meta.Completada)
                estado = "Completada";
            else if (meta.FechaLimite < ahora)
                estado = "Vencida";
            else if (meta.DiasRestantes <= 7)
                estado = "Por vencer";
            else
                estado = "En progreso";

            return new MetaResponseDTO
            {
                Id = meta.Id,
                Nombre = meta.Nombre,
                Descripcion = meta.Descripcion,
                MontoObjetivo = meta.MontoObjetivo,
                MontoActual = meta.MontoActual,
                FechaLimite = meta.FechaLimite,
                FechaCreacion = meta.FechaCreacion,
                Completada = meta.Completada,
                Icono = meta.Icono,
                Color = meta.Color,
                PorcentajeCompletado = meta.PorcentajeCompletado,
                MontoRestante = meta.MontoRestante,
                DiasRestantes = meta.DiasRestantes,
                Estado = estado,
                Aportes = meta.Aportes?.OrderByDescending(a => a.FechaAporte)
                    .Select(a => new AporteMetaResponseDTO
                    {
                        Id = a.Id,
                        Monto = a.Monto,
                        FechaAporte = a.FechaAporte,
                        Nota = a.Nota
                    }).ToList()
            };
        }
    }
}
