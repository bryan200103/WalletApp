using System.ComponentModel.DataAnnotations;

namespace WalletApi.DTOs
{
    public class MetaDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El monto objetivo es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal MontoObjetivo { get; set; }

        [Required(ErrorMessage = "La fecha límite es obligatoria")]
        public DateTime FechaLimite { get; set; }

        [StringLength(50)]
        public string? Icono { get; set; }

        [StringLength(7)]
        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Color inválido (formato: #RRGGBB)")]
        public string? Color { get; set; }
    }


    public class MetaResponseDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal MontoObjetivo { get; set; }
        public decimal MontoActual { get; set; }
        public DateTime FechaLimite { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Completada { get; set; }
        public string? Icono { get; set; }
        public string? Color { get; set; }

        // Propiedades calculadas
        public decimal PorcentajeCompletado { get; set; }
        public decimal MontoRestante { get; set; }
        public int DiasRestantes { get; set; }
        public string Estado { get; set; } = string.Empty;

        // Lista de aportes
        public List<AporteMetaResponseDTO>? Aportes { get; set; }
    }

    public class AporteMetaDTO
    {
        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        [StringLength(200)]
        public string? Nota { get; set; }
    }

    public class AporteMetaResponseDTO
    {
        public int Id { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaAporte { get; set; }
        public string? Nota { get; set; }
    }
    public class MetasResumenDTO
    {
        public int TotalMetas { get; set; }
        public int MetasCompletadas { get; set; }
        public int MetasEnProgreso { get; set; }
        public int MetasVencidas { get; set; }
        public decimal TotalAhorrado { get; set; }
        public decimal TotalObjetivo { get; set; }
        public decimal PorcentajeGeneral { get; set; }
    }
}
