using crudcomdb.Models;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Pet> Pets { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<InteresseAdocao> Interesses { get; set; }
        public DbSet<PetImagem> PetImagens { get; set; }
        public DbSet<ItemDoacao> ItensDoacao { get; set; }
        public DbSet<ItemImagem> ItemImagens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relacionamento Pet -> Imagens
            modelBuilder.Entity<Pet>()
                .HasMany(p => p.Imagens)
                .WithOne()
                .HasForeignKey(i => i.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relacionamento Pet -> UsuarioDoador
            modelBuilder.Entity<Pet>()
                .HasOne(p => p.UsuarioDoador)
                .WithMany()
                .HasForeignKey(p => p.UsuarioDoadorId);

            // Relacionamento ItemDoacao -> Imagens
            modelBuilder.Entity<ItemDoacao>()
                .HasMany(i => i.Imagens)
                .WithOne()
                .HasForeignKey(img => img.ItemDoacaoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relacionamento ItemDoacao -> UsuarioDoador
            modelBuilder.Entity<ItemDoacao>()
                .HasOne(i => i.UsuarioDoador)
                .WithMany()
                .HasForeignKey(i => i.UsuarioDoadorId);
        }
    }
}