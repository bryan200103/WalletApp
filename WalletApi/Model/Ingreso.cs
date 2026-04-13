namespace WalletApi.Model
{
    public class Ingreso
    {
        public int Id { get; set; }
        public decimal Monto { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        //public string Categoria { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public bool EsRecurrente { get; set; } = false;

        // Foreign Key
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        public int? CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }
    }
}
