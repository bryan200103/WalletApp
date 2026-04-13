using Microsoft.EntityFrameworkCore;
using WalletApi.Model;
namespace WalletApi.Data

{
    public class WalletDbContext : DbContext
    {
        public WalletDbContext(DbContextOptions<WalletDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Ingreso> Ingresos { get; set; }
        public DbSet<Gasto> Gastos { get; set; }
        public DbSet<DeudaMensual> Deudas { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Meta> Metas { get; set; }
        public DbSet<AporteMeta> AportesMetas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ingreso>()
                .Property(i => i.Monto)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Gasto>()
                .Property(g => g.Monto)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<DeudaMensual>()
                .Property(d => d.MontoTotal)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<DeudaMensual>()
                .Property(d => d.MontoCuota)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Meta>(entity =>
            {
                entity.HasIndex(m => m.UsuarioId);
                entity.HasIndex(m => m.Completada);
                entity.HasIndex(m => m.FechaLimite);

                entity.HasOne(m => m.Usuario)
                    .WithMany()
                    .HasForeignKey(m => m.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AporteMeta>(entity =>
            {
                entity.HasIndex(a => a.MetaId);
                entity.HasIndex(a => a.FechaAporte);

                entity.HasOne(a => a.Meta)
                    .WithMany(m => m.Aportes)
                    .HasForeignKey(a => a.MetaId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Usuario)
                    .WithMany()
                    .HasForeignKey(a => a.UsuarioId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}
