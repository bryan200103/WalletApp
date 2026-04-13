using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WalletApi.Model
{
    public class DeudaMensual
    {
        public int Id { get; set; }

        [Required]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public string Acreedor { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoTotal { get; set; }

        public int CantidadCuotas { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoCuota { get; set; }

        public int CuotasPagadas { get; set; } = 0;

        public DateTime FechaInicio { get; set; }

        public DateTime ProximoVencimiento { get; set; }

        // Foreign Key
        public int UsuarioId { get; set; }

        // Navegación
        public Usuario? Usuario { get; set; }

        // Propiedades calculadas (no se guardan en BD)
        [NotMapped]
        public decimal SaldoPendiente => MontoTotal - (MontoCuota * CuotasPagadas);

        [NotMapped]
        public int CuotasRestantes => CantidadCuotas - CuotasPagadas;

        [NotMapped]
        public bool EstaPagada => CuotasPagadas >= CantidadCuotas;
    }
}
