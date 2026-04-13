namespace WalletApi.DTOs
{
    public class ReportesDTO
    {
        public class ResumenFinancieroDTO
        {
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }
            public decimal TotalIngresos { get; set; }
            public decimal TotalGastos { get; set; }
            public decimal Balance { get; set; }
            public decimal TotalDeudasPendientes { get; set; }
            public decimal TotalDeudasPagadas { get; set; }
            public decimal AhorroEnMetas { get; set; }
            public int MetasActivas { get; set; }
            public decimal PromedioDiarioGastos { get; set; }
            public decimal PromedioDiarioIngresos { get; set; }
            public int NumeroTransacciones { get; set; }
            public string? CategoriaMayorGasto { get; set; }
        }

        public class EstadisticaCategoriaDTO
        {
            public int CategoriaId { get; set; }
            public string CategoriaNombre { get; set; } = string.Empty;
            public string? CategoriaIcono { get; set; }
            public string? CategoriaColor { get; set; }
            public decimal Total { get; set; }
            public int Cantidad { get; set; }
            public decimal Porcentaje { get; set; }
            public decimal Promedio { get; set; }
        }

        public class TendenciaMensualDTO
        {
            public int Anio { get; set; }
            public int Mes { get; set; }
            public string NombreMes { get; set; } = string.Empty;
            public decimal TotalIngresos { get; set; }
            public decimal TotalGastos { get; set; }
            public decimal Balance { get; set; }
            public decimal Ahorro { get; set; }
            public decimal PorcentajeAhorro { get; set; }
        }

        public class TendenciaDiariaDTO
        {
            public DateTime Fecha { get; set; }
            public string DiaSemana { get; set; } = string.Empty;
            public decimal TotalIngresos { get; set; }
            public decimal TotalGastos { get; set; }
            public decimal Balance { get; set; }
        }

        public class FlujoCajaDTO
        {
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }
            public decimal SaldoInicial { get; set; }
            public decimal TotalEntradas { get; set; }
            public decimal TotalSalidas { get; set; }
            public decimal PagosDeudas { get; set; }
            public decimal AportesAhorro { get; set; }
            public decimal FlujoNeto { get; set; }
            public decimal SaldoFinal { get; set; }
            public List<MovimientoFlujoCajaDTO> Movimientos { get; set; } = new();
        }

        public class MovimientoFlujoCajaDTO
        {
            public DateTime Fecha { get; set; }
            public string Tipo { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public string? Categoria { get; set; }
            public decimal Monto { get; set; }
            public decimal SaldoAcumulado { get; set; }
        }

        public class ProyeccionFinancieraDTO
        {
            public decimal PromedioIngresosUltimos3Meses { get; set; }
            public decimal PromedioGastosUltimos3Meses { get; set; }
            public decimal ProyeccionIngresosProximoMes { get; set; }
            public decimal ProyeccionGastosProximoMes { get; set; }
            public decimal ProyeccionBalance { get; set; }
            public List<ProyeccionMensualDTO> ProyeccionMeses { get; set; } = new();
        }

        public class ProyeccionMensualDTO
        {
            public int Mes { get; set; }
            public int Anio { get; set; }
            public string NombreMes { get; set; } = string.Empty;
            public decimal IngresosProyectados { get; set; }
            public decimal GastosProyectados { get; set; }
            public decimal BalanceProyectado { get; set; }
        }

        public class ComparativaPeriodosDTO
        {
            public PeriodoFinancieroDTO PeriodoActual { get; set; } = new();
            public PeriodoFinancieroDTO PeriodoAnterior { get; set; } = new();
            public decimal VariacionIngresos { get; set; }
            public decimal VariacionGastos { get; set; }
            public decimal VariacionBalance { get; set; }
            public decimal PorcentajeVariacionIngresos { get; set; }
            public decimal PorcentajeVariacionGastos { get; set; }
        }

        public class PeriodoFinancieroDTO
        {
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }
            public decimal TotalIngresos { get; set; }
            public decimal TotalGastos { get; set; }
            public decimal Balance { get; set; }
        }

        public class TopMovimientosDTO
        {
            public List<MovimientoDetalleDTO> TopGastos { get; set; } = new();
            public List<MovimientoDetalleDTO> TopIngresos { get; set; } = new();
        }

        public class MovimientoDetalleDTO
        {
            public int Id { get; set; }
            public string Descripcion { get; set; } = string.Empty;
            public decimal Monto { get; set; }
            public DateTime Fecha { get; set; }
            public string? Categoria { get; set; }
        }

        public class ReporteRecurrentesDTO
        {
            public decimal TotalIngresosRecurrentes { get; set; }
            public decimal TotalGastosRecurrentes { get; set; }
            public decimal BalanceRecurrente { get; set; }
            public List<MovimientoRecurrenteDTO> IngresosRecurrentes { get; set; } = new();
            public List<MovimientoRecurrenteDTO> GastosRecurrentes { get; set; } = new();
        }

        public class MovimientoRecurrenteDTO
        {
            public int Id { get; set; }
            public string Descripcion { get; set; } = string.Empty;
            public decimal Monto { get; set; }
            public string? Categoria { get; set; }
        }
    }
}
