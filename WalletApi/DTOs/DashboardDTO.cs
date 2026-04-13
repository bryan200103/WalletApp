namespace WalletApi.DTOs
{
    public class DashboardDTO
    {
        public decimal TotalIngresos { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal TotalDeudas { get; set; }
        public decimal SaldoDisponible { get; set; }
        public List<IngresoDTO> UltimosIngresos { get; set; } = new();
        public List<GastoDTO> UltimosGastos { get; set; } = new();
        public List<DeudaDTO> DeudasPendientes { get; set; } = new();

    }
    public class IngresoDTO
    {
        public decimal Monto { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int? CategoriaId { get; set; }
        public DateTime? Fecha { get; set; }
        public bool EsRecurrente { get; set; } = false;
    }
    public class GastoDTO
    {
        public decimal Monto { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int? CategoriaId { get; set; }
        public DateTime Fecha { get; set; }
    }
    public class DeudaDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public int DiaVencimiento { get; set; }
        public bool EstaPagado { get; set; }
    }
    public class DeudaCreateDto
    {
        public string Descripcion { get; set; } = string.Empty;
        public decimal MontoTotal { get; set; }
        public int CantidadCuotas { get; set; }
        public int CuotasPagadas { get; set; } = 0;
        public decimal MontoCuota { get; set; }
        public DateTime ProximoVencimiento { get; set; }
    }
    public class DeudaUpdateDto
    {
        public string Descripcion { get; set; } = string.Empty;
        public decimal MontoTotal { get; set; }
        public int CantidadCuotas { get; set; }
        public int CuotasPagadas { get; set; }
        public decimal MontoCuota { get; set; }
        public DateTime ProximoVencimiento { get; set; }
    }
    public class UsuarioRegistroDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // DTOs/UsuarioUpdateDTO.cs
    public class UsuarioUpdateDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
