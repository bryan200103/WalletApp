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
    public class DeudasController : ControllerBase
    {
        private readonly WalletDbContext _context;

        public DeudasController(WalletDbContext context)
        {
            _context = context;
        }

        // GET: api/deudas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeudaMensual>>> GetDeudas()
        {
            var userId = User.GetUserId();
            return await _context.Deudas
                .Where(d => d.UsuarioId == userId)
                .OrderBy(d => d.ProximoVencimiento)
                .ToListAsync();
        }

        // GET: api/deudas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DeudaMensual>> GetDeuda(int id)
        {
            var userId = User.GetUserId();
            var deuda = await _context.Deudas
                .FirstOrDefaultAsync(d => d.Id == id && d.UsuarioId == userId);

            if (deuda == null)
                return NotFound();

            return deuda;
        }

        // GET: api/deudas/pendientes
        [HttpGet("pendientes")]
        public async Task<ActionResult<IEnumerable<DeudaMensual>>> GetDeudasPendientes()
        {
            var userId = User.GetUserId();
            var deudas = await _context.Deudas
                .Where(d => d.UsuarioId == userId)
                .ToListAsync();

            var pendientes = deudas.Where(d => !d.EstaPagada).OrderBy(d => d.ProximoVencimiento);

            return Ok(pendientes);
        }

        // POST: api/deudas
        [HttpPost]
        public async Task<ActionResult<DeudaMensual>> CrearDeuda(DeudaCreateDto dto)
        {
            var userId = User.GetUserId();

            var deuda = new DeudaMensual
            {
                UsuarioId = userId,
                Descripcion = dto.Descripcion,
                MontoTotal = dto.MontoTotal,
                CantidadCuotas = dto.CantidadCuotas,
                CuotasPagadas = dto.CuotasPagadas,
                MontoCuota = dto.MontoCuota,
                ProximoVencimiento = dto.ProximoVencimiento
            };

            if (deuda.MontoCuota == 0 && deuda.CantidadCuotas > 0)
            {
                deuda.MontoCuota = deuda.MontoTotal / deuda.CantidadCuotas;
            }

            _context.Deudas.Add(deuda);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDeuda), new { id = deuda.Id }, deuda);
        }

        // PUT: api/deudas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarDeuda(int id, DeudaUpdateDto dto)
        {
            var userId = User.GetUserId();
            var deuda = await _context.Deudas
                .FirstOrDefaultAsync(d => d.Id == id && d.UsuarioId == userId);

            if (deuda == null)
                return NotFound();

            deuda.Descripcion = dto.Descripcion;
            deuda.MontoTotal = dto.MontoTotal;
            deuda.CantidadCuotas = dto.CantidadCuotas;
            deuda.CuotasPagadas = dto.CuotasPagadas;
            deuda.MontoCuota = dto.MontoCuota;
            deuda.ProximoVencimiento = dto.ProximoVencimiento;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Deuda actualizada", deuda });
        }

        // POST: api/deudas/5/pagar
        [HttpPost("{id}/pagar")]
        public async Task<IActionResult> RegistrarPago(int id)
        {
            var userId = User.GetUserId();
            var deuda = await _context.Deudas
                .FirstOrDefaultAsync(d => d.Id == id && d.UsuarioId == userId);

            if (deuda == null)
                return NotFound();

            if (deuda.EstaPagada)
                return BadRequest("La deuda ya está completamente pagada.");

            deuda.CuotasPagadas++;

            var gasto = new Gasto
            {
                UsuarioId = userId,
                Monto = deuda.MontoCuota,
                Descripcion = $"Cuota {deuda.CuotasPagadas}/{deuda.CantidadCuotas} - {deuda.Descripcion}",
                Fecha = DateTime.Now
            };
            _context.Gastos.Add(gasto);

            if (!deuda.EstaPagada)
            {
                deuda.ProximoVencimiento = deuda.ProximoVencimiento.AddMonths(1);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Pago registrado",
                cuotasPagadas = deuda.CuotasPagadas,
                cuotasRestantes = deuda.CuotasRestantes,
                saldoPendiente = deuda.SaldoPendiente,
                estaPagada = deuda.EstaPagada,
                gastoRegistrado = gasto.Monto
            });
        }

        // DELETE: api/deudas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarDeuda(int id)
        {
            var userId = User.GetUserId();
            var deuda = await _context.Deudas
                .FirstOrDefaultAsync(d => d.Id == id && d.UsuarioId == userId);

            if (deuda == null)
                return NotFound();

            _context.Deudas.Remove(deuda);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Deuda eliminada" });
        }
    }
}
