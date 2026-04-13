namespace WalletApi.Model
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Navegación
        public ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();
        public ICollection<Gasto> Gastos { get; set; } = new List<Gasto>();
        public ICollection<DeudaMensual> Deudas { get; set; } = new List<DeudaMensual>();
    }
}
