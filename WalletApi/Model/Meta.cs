using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WalletApi.Model
{
    public class Meta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoObjetivo { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoActual { get; set; } = 0;

        [Required]
        public DateTime FechaLimite { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public bool Completada { get; set; } = false;

        [StringLength(50)]
        public string? Icono { get; set; }

        [StringLength(7)]
        public string? Color { get; set; }

        // Relación con Usuario
        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }

        // Relación con Aportes
        public virtual ICollection<AporteMeta>? Aportes { get; set; }

        // Propiedades calculadas (no mapeadas)
        [NotMapped]
        public decimal PorcentajeCompletado =>
            MontoObjetivo > 0 ? Math.Round((MontoActual / MontoObjetivo) * 100, 2) : 0;

        [NotMapped]
        public decimal MontoRestante => MontoObjetivo - MontoActual;

        [NotMapped]
        public int DiasRestantes => (FechaLimite - DateTime.Now).Days;
    }
}
