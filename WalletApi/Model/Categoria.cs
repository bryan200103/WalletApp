using System.ComponentModel.DataAnnotations;

namespace WalletApi.Model
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(10)]
        public string Tipo { get; set; } = string.Empty; // "Gasto" o "Ingreso"

        public string? Icono { get; set; } // Opcional: "🍔", "🚗", etc.

        public string? Color { get; set; } // Opcional: "#FF5733"

        // Relaciones
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public ICollection<Gasto>? Gastos { get; set; }
        public ICollection<Ingreso>? Ingresos { get; set; }
    }
}
