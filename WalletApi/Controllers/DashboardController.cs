using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WalletApi.Data;
using WalletApi.Extensions;

namespace WalletApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly WalletDbContext _context;

        public DashboardController(WalletDbContext context)
        {
            _context = context;
        }

        // GET: api/dashboard
        [HttpGet]
        public async Task<ActionResult> GetDashboard()
        {
            var userId = User.GetUserId();

            var usuario = await _context.Usuarios.FindAsync(userId);
            if (usuario == null)
                return NotFound("Usuario no encontrado");

            var ingresos = await _context.Ingresos
                .Where(i => i.UsuarioId == userId)
                .SumAsync(i => i.Monto);

            var gastos = await _context.Gastos
                .Where(g => g.UsuarioId == userId)
                .SumAsync(g => g.Monto);

            var deudas = await _context.Deudas
                .Where(d => d.UsuarioId == userId)
                .ToListAsync();

            var deudasPendientes = deudas.Where(d => !d.EstaPagada).ToList();
            var totalDeudas = deudasPendientes.Sum(d => d.SaldoPendiente);

            return Ok(new
            {
                usuarioId = userId,
                nombreUsuario = usuario.Nombre,
                totalIngresos = ingresos,
                totalGastos = gastos,
                balance = ingresos - gastos,
                totalDeudas,
                cantidadDeudasPendientes = deudasPendientes.Count
            });
        }

        // GET: api/dashboard/resumen-mensual?mes=1&anio=2025
        [HttpGet("resumen-mensual")]
        public async Task<ActionResult> GetResumenMensual(int? mes, int? anio)
        {
            var userId = User.GetUserId();
            var mesActual = mes ?? DateTime.Now.Month;
            var anioActual = anio ?? DateTime.Now.Year;

            var ingresos = await _context.Ingresos
                .Where(i => i.UsuarioId == userId &&
                            i.Fecha.Month == mesActual &&
                            i.Fecha.Year == anioActual)
                .SumAsync(i => i.Monto);

            var gastos = await _context.Gastos
                .Where(g => g.UsuarioId == userId &&
                            g.Fecha.Month == mesActual &&
                            g.Fecha.Year == anioActual)
                .SumAsync(g => g.Monto);

            var deudasVencenEsteMes = await _context.Deudas
                .Where(d => d.UsuarioId == userId &&
                            d.ProximoVencimiento.Month == mesActual &&
                            d.ProximoVencimiento.Year == anioActual)
                .ToListAsync();

            var deudasPendientes = deudasVencenEsteMes.Where(d => !d.EstaPagada).ToList();

            return Ok(new
            {
                mes = mesActual,
                anio = anioActual,
                ingresos,
                gastos,
                balance = ingresos - gastos,
                deudasDelMes = deudasPendientes.Sum(d => d.MontoCuota),
                cantidadDeudasVencen = deudasPendientes.Count
            });
        }
    }
}
