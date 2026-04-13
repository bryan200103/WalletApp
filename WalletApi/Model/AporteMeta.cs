using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WalletApi.Model
{
    public class AporteMeta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        public DateTime FechaAporte { get; set; } = DateTime.Now;

        [StringLength(200)]
        public string? Nota { get; set; }

        // Relación con Meta
        [Required]
        public int MetaId { get; set; }

        [ForeignKey("MetaId")]
        public virtual Meta? Meta { get; set; }

        // Relación con Usuario
        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }
    }
}
