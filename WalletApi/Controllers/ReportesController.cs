using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WalletApi.Data;
using WalletApi.Extensions;
using static WalletApi.DTOs.ReportesDTO;

namespace WalletApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private readonly WalletDbContext _context;
        private static readonly CultureInfo _culturaES = new("es-ES");

        public ReportesController(WalletDbContext context)
        {
            _context = context;
        }

        // GET: api/Reportes/resumen
        [HttpGet("resumen")]
        public async Task<ActionResult<ResumenFinancieroDTO>> GetResumenFinanciero(
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin)
        {
            var usuarioId = User.GetUserId();

            var inicio = fechaInicio ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var fin = fechaFin ?? DateTime.Now;

            var totalIngresos = await _context.Ingresos
                .Where(i => i.UsuarioId == usuarioId && i.Fecha >= inicio && i.Fecha <= fin)
                .SumAsync(i => (decimal?)i.Monto) ?? 0;

            var totalGastos = await _context.Gastos
                .Where(g => g.UsuarioId == usuarioId && g.Fecha >= inicio && g.Fecha <= fin)
                .SumAsync(g => (decimal?)g.Monto) ?? 0;

            // ✅ CORREGIDO: Usar propiedades de DeudaMensual
            // Deudas pendientes = suma del saldo pendiente de deudas no completadas
            var deudasPendientes = await _context.Deudas
                .Where(d => d.UsuarioId == usuarioId && d.CuotasPagadas < d.CantidadCuotas)
                .SumAsync(d => (decimal?)(d.MontoTotal - (d.MontoCuota * d.CuotasPagadas))) ?? 0;

            // Total pagado en deudas en el período (aproximación por cuotas)
            // Como no hay FechaPago, calculamos las deudas que vencieron en el período
            var cuotasPagadasEnPeriodo = await _context.Deudas
                .Where(d => d.UsuarioId == usuarioId &&
                       d.FechaInicio <= fin &&
                       d.CuotasPagadas > 0)
                .SumAsync(d => (decimal?)(d.MontoCuota * d.CuotasPagadas)) ?? 0;

            var metasActivas = await _context.Metas
                .Where(m => m.UsuarioId == usuarioId && !m.Completada)
                .CountAsync();

            var ahorroMetas = await _context.Metas
                .Where(m => m.UsuarioId == usuarioId)
                .SumAsync(m => (decimal?)m.MontoActual) ?? 0;

            var diasEnPeriodo = (fin - inicio).Days + 1;
            var promedioDiarioGastos = diasEnPeriodo > 0 ? totalGastos / diasEnPeriodo : 0;
            var promedioDiarioIngresos = diasEnPeriodo > 0 ? totalIngresos / diasEnPeriodo : 0;

            var numeroTransacciones = await _context.Gastos
                .Where(g => g.UsuarioId == usuarioId && g.Fecha >= inicio && g.Fecha <= fin)
                .CountAsync() +
                await _context.Ingresos
                .Where(i => i.UsuarioId == usuarioId && i.Fecha >= inicio && i.Fecha <= fin)
                .CountAsync();

            var categoriaTopGasto = await _context.Gastos
                .Where(g => g.UsuarioId == usuarioId && g.Fecha >= inicio && g.Fecha <= fin)
                .GroupBy(g => g.CategoriaId)
                .Select(g => new { CategoriaId = g.Key, Total = g.Sum(x => x.Monto) })
                .OrderByDescending(g => g.Total)
                .FirstOrDefaultAsync();

            string? nombreCategoriaTop = null;
            if (categoriaTopGasto != null)
            {
                var categoria = await _context.Categorias.FindAsync(categoriaTopGasto.CategoriaId);
                nombreCategoriaTop = categoria?.Nombre;
            }

            return Ok(new ResumenFinancieroDTO
            {
                FechaInicio = inicio,
                FechaFin = fin,
                TotalIngresos = totalIngresos,
                TotalGastos = totalGastos,
                Balance = totalIngresos - totalGastos,
                TotalDeudasPendientes = deudasPendientes,
                TotalDeudasPagadas = cuotasPagadasEnPeriodo,
                AhorroEnMetas = ahorroMetas,
                MetasActivas = metasActivas,
                PromedioDiarioGastos = promedioDiarioGastos,
                PromedioDiarioIngresos = promedioDiarioIngresos,
                NumeroTransacciones = numeroTransacciones,
                CategoriaMayorGasto = nombreCategoriaTop
            });
        }

        // GET: api/Reportes/estadisticas-categorias
        [HttpGet("estadisticas-categorias")]
        public async Task<ActionResult<IEnumerable<EstadisticaCategoriaDTO>>> GetEstadisticasCategorias(
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] string tipo = "gasto")
        {
            var usuarioId = User.GetUserId();

            var inicio = fechaInicio ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var fin = fechaFin ?? DateTime.Now;

            if (tipo.ToLower() == "ingreso")
            {
                var totalIngresos = await _context.Ingresos
                    .Where(i => i.UsuarioId == usuarioId && i.Fecha >= inicio && i.Fecha <= fin)
                    .SumAsync(i => (decimal?)i.Monto) ?? 0;

                var estadisticas = await _context.Ingresos
                    .Where(i => i.UsuarioId == usuarioId && i.Fecha >= inicio && i.Fecha <= fin && i.CategoriaId != null)
                    .GroupBy(i => new { i.CategoriaId, i.Categoria!.Nombre, i.Categoria.Icono, i.Categoria.Color })
                    .Select(g => new EstadisticaCategoriaDTO
                    {
                        CategoriaId = g.Key.CategoriaId ?? 0,
                        CategoriaNombre = g.Key.Nombre ?? "Sin categoría",
                        CategoriaIcono = g.Key.Icono,
                        CategoriaColor = g.Key.Color,
                        Total = g.Sum(i => i.Monto),
                        Cantidad = g.Count(),
                        Promedio = g.Average(i => i.Monto),
                        Porcentaje = totalIngresos > 0 ? Math.Round((g.Sum(i => i.Monto) / totalIngresos) * 100, 2) : 0
                    })
                    .OrderByDescending(e => e.Total)
                    .ToListAsync();

                return Ok(estadisticas);
            }
            else
            {
                var totalGastos = await _context.Gastos
                    .Where(g => g.UsuarioId == usuarioId && g.Fecha >= inicio && g.Fecha <= fin)
                    .SumAsync(g => (decimal?)g.Monto) ?? 0;

                var estadisticas = await _context.Gastos
                    .Where(g => g.UsuarioId == usuarioId && g.Fecha >= inicio && g.Fecha <= fin && g.CategoriaId != null)
                    .GroupBy(g => new { g.CategoriaId, g.Categoria!.Nombre, g.Categoria.Icono, g.Categoria.Color })
                    .Select(g => new EstadisticaCategoriaDTO
                    {
                        CategoriaId = g.Key.CategoriaId ?? 0,
                        CategoriaNombre = g.Key.Nombre ?? "Sin categoría",
                        CategoriaIcono = g.Key.Icono,
                        CategoriaColor = g.Key.Color,
                        Total = g.Sum(x => x.Monto),
                        Cantidad = g.Count(),
                        Promedio = g.Average(x => x.Monto),
                        Porcentaje = totalGastos > 0 ? Math.Round((g.Sum(x => x.Monto) / totalGastos) * 100, 2) : 0
                    })
                    .OrderByDescending(e => e.Total)
                    .ToListAsync();

                return Ok(estadisticas);
            }
        }

        // GET: api/Reportes/tendencia-mensual
        [HttpGet("tendencia-mensual")]
        public async Task<ActionResult<IEnumerable<TendenciaMensualDTO>>> GetTendenciaMensual(
            [FromQuery] int meses = 6)
        {
            var usuarioId = User.GetUserId();
            var fechaInicio = DateTime.Now.AddMonths(-meses + 1);
            fechaInicio = new DateTime(fechaInicio.Year, fechaInicio.Month, 1);

            var tendencias = new List<TendenciaMensualDTO>();

            for (int i = 0; i < meses; i++)
            {
                var mesActual = fechaInicio.AddMonths(i);
                var inicioMes = new DateTime(mesActual.Year, mesActual.Month, 1);
                var finMes = inicioMes.AddMonths(1).AddDays(-1);

                var ingresos = await _context.Ingresos
                    .Where(x => x.UsuarioId == usuarioId && x.Fecha >= inicioMes && x.Fecha <= finMes)
                    .SumAsync(x => (decimal?)x.Monto) ?? 0;

                var gastos = await _context.Gastos
                    .Where(x => x.UsuarioId == usuarioId && x.Fecha >= inicioMes && x.Fecha <= finMes)
                    .SumAsync(x => (decimal?)x.Monto) ?? 0;

                var ahorros = await _context.AportesMetas
                    .Where(x => x.UsuarioId == usuarioId && x.FechaAporte >= inicioMes && x.FechaAporte <= finMes)
                    .SumAsync(x => (decimal?)x.Monto) ?? 0;

                tendencias.Add(new TendenciaMensualDTO
                {
                    Anio = mesActual.Year,
                    Mes = mesActual.Month,
                    NombreMes = mesActual.ToString("MMMM", _culturaES),
                    TotalIngresos = ingresos,
                    TotalGastos = gastos,
                    Balance = ingresos - gastos,
                    Ahorro = ahorros,
                    PorcentajeAhorro = ingresos > 0 ? Math.Round((ahorros / ingresos) * 100, 2) : 0
                });
            }

            return Ok(tendencias);
        }

        // GET: api/Reportes/tendencia-diaria
        [HttpGet("tendencia-diaria")]
        public async Task<ActionResult<IEnumerable<TendenciaDiariaDTO>>> GetTendenciaDiaria(
            [FromQuery] int dias = 30)
        {
            var usuarioId = User.GetUserId();
            var fechaInicio = DateTime.Now.Date.AddDays(-dias + 1);

            var tendencias = new List<TendenciaDiariaDTO>();

            for (int i = 0; i < dias; i++)
            {
                var diaActual = fechaInicio.AddDays(i);

                var ingresos = await _context.Ingresos
                    .Where(x => x.UsuarioId == usuarioId && x.Fecha.Date == diaActual)
                    .SumAsync(x => (decimal?)x.Monto) ?? 0;

                var gastos = await _context.Gastos
                    .Where(x => x.UsuarioId == usuarioId && x.Fecha.Date == diaActual)
                    .SumAsync(x => (decimal?)x.Monto) ?? 0;

                tendencias.Add(new TendenciaDiariaDTO
                {
                    Fecha = diaActual,
                    DiaSemana = diaActual.ToString("dddd", _culturaES),
                    TotalIngresos = ingresos,
                    TotalGastos = gastos,
                    Balance = ingresos - gastos
                });
            }

            return Ok(tendencias);
        }

        // GET: api/Reportes/flujo-caja
        [HttpGet("flujo-caja")]
        public async Task<ActionResult<FlujoCajaDTO>> GetFlujoCaja(
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin)
        {
            var usuarioId = User.GetUserId();

            var inicio = fechaInicio ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var fin = fechaFin ?? DateTime.Now;

            var ingresosAnteriores = await _context.Ingresos
                .Where(i => i.UsuarioId == usuarioId && i.Fecha < inicio)
                .SumAsync(i => (decimal?)i.Monto) ?? 0;

            var gastosAnteriores = await _context.Gastos
                .Where(g => g.UsuarioId == usuarioId && g.Fecha < inicio)
                .SumAsync(g => (decimal?)g.Monto) ?? 0;

            var saldoInicial = ingresosAnteriores - gastosAnteriores;

            var ingresosPeriodo = await _context.Ingresos
                .Where(i => i.UsuarioId == usuarioId && i.Fecha >= inicio && i.Fecha <= fin)
                .SumAsync(i => (decimal?)i.Monto) ?? 0;

            var gastosPeriodo = await _context.Gastos
                .Where(g => g.UsuarioId == usuarioId && g.Fecha >= inicio && g.Fecha <= fin)
                .SumAsync(g => (decimal?)g.Monto) ?? 0;

            // ✅ CORREGIDO: Pagos de deudas basado en cuotas con vencimiento en el período
            var pagosDeudas = await _context.Deudas
                .Where(d => d.UsuarioId == usuarioId &&
                       d.ProximoVencimiento >= inicio &&
                       d.ProximoVencimiento <= fin &&
                       d.CuotasPagadas > 0)
                .SumAsync(d => (decimal?)d.MontoCuota) ?? 0;

            var ahorrosMetas = await _context.AportesMetas
                .Where(a => a.UsuarioId == usuarioId && a.FechaAporte >= inicio && a.FechaAporte <= fin)
                .SumAsync(a => (decimal?)a.Monto) ?? 0;

            var flujoNeto = ingresosPeriodo - gastosPeriodo;
            var saldoFinal = saldoInicial + flujoNeto;

            return Ok(new FlujoCajaDTO
            {
                FechaInicio = inicio,
                FechaFin = fin,
                SaldoInicial = saldoInicial,
                TotalEntradas = ingresosPeriodo,
                TotalSalidas = gastosPeriodo,
                PagosDeudas = pagosDeudas,
                AportesAhorro = ahorrosMetas,
                FlujoNeto = flujoNeto,
                SaldoFinal = saldoFinal
            });
        }

        // GET: api/Reportes/proyeccion
        [HttpGet("proyeccion")]
        public async Task<ActionResult<ProyeccionFinancieraDTO>> GetProyeccion([FromQuery] int mesesProyeccion = 3)
        {
            var usuarioId = User.GetUserId();
            var hoy = DateTime.Now;

            var hace3Meses = hoy.AddMonths(-3);

            var promedioIngresos = await _context.Ingresos
                .Where(i => i.UsuarioId == usuarioId && i.Fecha >= hace3Meses)
                .GroupBy(i => new { i.Fecha.Year, i.Fecha.Month })
                .Select(g => g.Sum(i => i.Monto))
                .ToListAsync();

            var promedioGastos = await _context.Gastos
                .Where(g => g.UsuarioId == usuarioId && g.Fecha >= hace3Meses)
                .GroupBy(g => new { g.Fecha.Year, g.Fecha.Month })
                .Select(g => g.Sum(x => x.Monto))
                .ToListAsync();

            var avgIngresos = promedioIngresos.Any() ? promedioIngresos.Average() : 0;
            var avgGastos = promedioGastos.Any() ? promedioGastos.Average() : 0;

            var proyecciones = new List<ProyeccionMensualDTO>();
            for (int i = 1; i <= mesesProyeccion; i++)
            {
                var mesProyectado = hoy.AddMonths(i);
                proyecciones.Add(new ProyeccionMensualDTO
                {
                    Mes = mesProyectado.Month,
                    Anio = mesProyectado.Year,
                    NombreMes = mesProyectado.ToString("MMMM yyyy", _culturaES),
                    IngresosProyectados = avgIngresos,
                    GastosProyectados = avgGastos,
                    BalanceProyectado = avgIngresos - avgGastos
                });
            }

            return Ok(new ProyeccionFinancieraDTO
            {
                PromedioIngresosUltimos3Meses = avgIngresos,
                PromedioGastosUltimos3Meses = avgGastos,
                ProyeccionIngresosProximoMes = avgIngresos,
                ProyeccionGastosProximoMes = avgGastos,
                ProyeccionBalance = avgIngresos - avgGastos,
                ProyeccionMeses = proyecciones
            });
        }

        // GET: api/Reportes/comparativa
        [HttpGet("comparativa")]
        public async Task<ActionResult<ComparativaPeriodosDTO>> GetComparativa(
            [FromQuery] DateTime? fechaInicioPeriodo1,
            [FromQuery] DateTime? fechaFinPeriodo1)
        {
            var usuarioId = User.GetUserId();

            var finActual = fechaFinPeriodo1 ?? DateTime.Now;
            var inicioActual = fechaInicioPeriodo1 ?? new DateTime(finActual.Year, finActual.Month, 1);

            var diasPeriodo = (finActual - inicioActual).Days;
            var finAnterior = inicioActual.AddDays(-1);
            var inicioAnterior = finAnterior.AddDays(-diasPeriodo);

            var ingresosActual = await _context.Ingresos
                .Where(i => i.UsuarioId == usuarioId && i.Fecha >= inicioActual && i.Fecha <= finActual)
                .SumAsync(i => (decimal?)i.Monto) ?? 0;

            var gastosActual = await _context.Gastos
                .Where(g => g.UsuarioId == usuarioId && g.Fecha >= inicioActual && g.Fecha <= finActual)
                .SumAsync(g => (decimal?)g.Monto) ?? 0;

            var ingresosAnterior = await _context.Ingresos
                .Where(i => i.UsuarioId == usuarioId && i.Fecha >= inicioAnterior && i.Fecha <= finAnterior)
                .SumAsync(i => (decimal?)i.Monto) ?? 0;

            var gastosAnterior = await _context.Gastos
                .Where(g => g.UsuarioId == usuarioId && g.Fecha >= inicioAnterior && g.Fecha <= finAnterior)
                .SumAsync(g => (decimal?)g.Monto) ?? 0;

            var variacionIngresos = ingresosActual - ingresosAnterior;
            var variacionGastos = gastosActual - gastosAnterior;

            return Ok(new ComparativaPeriodosDTO
            {
                PeriodoActual = new PeriodoFinancieroDTO
                {
                    FechaInicio = inicioActual,
                    FechaFin = finActual,
                    TotalIngresos = ingresosActual,
                    TotalGastos = gastosActual,
                    Balance = ingresosActual - gastosActual
                },
                PeriodoAnterior = new PeriodoFinancieroDTO
                {
                    FechaInicio = inicioAnterior,
                    FechaFin = finAnterior,
                    TotalIngresos = ingresosAnterior,
                    TotalGastos = gastosAnterior,
                    Balance = ingresosAnterior - gastosAnterior
                },
                VariacionIngresos = variacionIngresos,
                VariacionGastos = variacionGastos,
                VariacionBalance = (ingresosActual - gastosActual) - (ingresosAnterior - gastosAnterior),
                PorcentajeVariacionIngresos = ingresosAnterior > 0 ? Math.Round((variacionIngresos / ingresosAnterior) * 100, 2) : 0,
                PorcentajeVariacionGastos = gastosAnterior > 0 ? Math.Round((variacionGastos / gastosAnterior) * 100, 2) : 0
            });
        }

        // GET: api/Reportes/top-movimientos
        [HttpGet("top-movimientos")]
        public async Task<ActionResult<TopMovimientosDTO>> GetTopMovimientos(
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] int top = 5)
        {
            var usuarioId = User.GetUserId();

            var inicio = fechaInicio ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var fin = fechaFin ?? DateTime.Now;

            var topGastos = await _context.Gastos
                .Where(g => g.UsuarioId == usuarioId && g.Fecha >= inicio && g.Fecha <= fin)
                .OrderByDescending(g => g.Monto)
                .Take(top)
                .Select(g => new MovimientoDetalleDTO
                {
                    Id = g.Id,
                    Descripcion = g.Descripcion ?? "",
                    Monto = g.Monto,
                    Fecha = g.Fecha,
                    Categoria = g.Categoria != null ? g.Categoria.Nombre : null
                })
                .ToListAsync();

            var topIngresos = await _context.Ingresos
                .Where(i => i.UsuarioId == usuarioId && i.Fecha >= inicio && i.Fecha <= fin)
                .OrderByDescending(i => i.Monto)
                .Take(top)
                .Select(i => new MovimientoDetalleDTO
                {
                    Id = i.Id,
                    Descripcion = i.Descripcion ?? "",
                    Monto = i.Monto,
                    Fecha = i.Fecha,
                    Categoria = i.Categoria != null ? i.Categoria.Nombre : null
                })
                .ToListAsync();

            return Ok(new TopMovimientosDTO
            {
                TopGastos = topGastos,
                TopIngresos = topIngresos
            });
        }

        // GET: api/Reportes/deudas-mensuales
        [HttpGet("deudas-mensuales")]
        public async Task<ActionResult<object>> GetReporteDeudas()
        {
            var usuarioId = User.GetUserId();

            var deudas = await _context.Deudas
                .Where(d => d.UsuarioId == usuarioId)
                .ToListAsync();

            var deudasActivas = deudas.Where(d => !d.EstaPagada).ToList();
            var deudasPagadas = deudas.Where(d => d.EstaPagada).ToList();

            var totalPendiente = deudasActivas.Sum(d => d.SaldoPendiente);
            var totalPagado = deudas.Sum(d => d.MontoCuota * d.CuotasPagadas);
            var proximosVencimientos = deudasActivas
                .Where(d => d.ProximoVencimiento <= DateTime.Now.AddDays(30))
                .OrderBy(d => d.ProximoVencimiento)
                .Select(d => new
                {
                    d.Id,
                    d.Descripcion,
                    d.Acreedor,
                    d.MontoCuota,
                    d.ProximoVencimiento,
                    d.CuotasRestantes,
                    DiasParaVencer = (d.ProximoVencimiento - DateTime.Now).Days
                })
                .ToList();

            return Ok(new
            {
                TotalDeudas = deudas.Count,
                DeudasActivas = deudasActivas.Count,
                DeudasPagadas = deudasPagadas.Count,
                TotalMontoPendiente = totalPendiente,
                TotalMontoPagado = totalPagado,
                CuotasMensualesActivas = deudasActivas.Sum(d => d.MontoCuota),
                ProximosVencimientos = proximosVencimientos
            });
        }
    }
}
