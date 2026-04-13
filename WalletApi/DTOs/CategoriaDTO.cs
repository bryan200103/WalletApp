namespace WalletApi.DTOs
{
    public class CategoriaDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; // "Gasto" o "Ingreso"
        public string? Icono { get; set; }
        public string? Color { get; set; }
    }
}
