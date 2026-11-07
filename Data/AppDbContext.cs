using crudcomdb.Models;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Data
{
    public class AppDbContext : DbContext, IDataProtectionKeyContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Pet> Pets { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<InteresseAdocao> Interesses { get; set; }
        public DbSet<PetImagem> PetImagens { get; set; }
        public DbSet<ItemDoacao> ItensDoacao { get; set; }
        public DbSet<ItemImagem> ItemImagens { get; set; }

        // Rede social, chat e termos
        public DbSet<Midia> Midias { get; set; }
        public DbSet<Postagem> Postagens { get; set; }
        public DbSet<Curtida> Curtidas { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<Amizade> Amizades { get; set; }
        public DbSet<Conversa> Conversas { get; set; }
        public DbSet<Mensagem> Mensagens { get; set; }
        public DbSet<TermoAdocao> TermosAdocao { get; set; }
        public DbSet<Depoimento> Depoimentos { get; set; }
        public DbSet<Notificacao> Notificacoes { get; set; }

        // Moderação, favoritos, questionário de adoção e avaliação de uso
        public DbSet<Denuncia> Denuncias { get; set; }
        public DbSet<Favorito> Favoritos { get; set; }
        public DbSet<PerfilAdotante> PerfisAdotante { get; set; }
        public DbSet<AvaliacaoUso> AvaliacoesUso { get; set; }

        // Chaves de criptografia do site (sessões e documentos): ficam no banco para não se perderem a cada publicação
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ----- Pets -----
            modelBuilder.Entity<Pet>()
                .HasMany(p => p.Imagens)
                .WithOne()
                .HasForeignKey(i => i.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Pet>()
                .HasOne(p => p.UsuarioDoador)
                .WithMany()
                .HasForeignKey(p => p.UsuarioDoadorId);

            modelBuilder.Entity<Pet>()
                .HasOne(p => p.Adotante)
                .WithMany()
                .HasForeignKey(p => p.AdotanteId)
                .OnDelete(DeleteBehavior.SetNull);

            // Registros antigos recebem a data da migração
            modelBuilder.Entity<Pet>().Property(p => p.DataCadastro).HasDefaultValueSql("now()");
            modelBuilder.Entity<Pet>().HasIndex(p => p.Status);

            // ----- Bazar -----
            modelBuilder.Entity<ItemDoacao>()
                .HasMany(i => i.Imagens)
                .WithOne()
                .HasForeignKey(img => img.ItemDoacaoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ItemDoacao>()
                .HasOne(i => i.UsuarioDoador)
                .WithMany()
                .HasForeignKey(i => i.UsuarioDoadorId);

            // ----- Comunidade -----
            modelBuilder.Entity<Postagem>()
                .HasOne(p => p.Autor).WithMany().HasForeignKey(p => p.AutorId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Postagem>()
                .HasOne(p => p.Pet).WithMany().HasForeignKey(p => p.PetId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Postagem>()
                .HasMany(p => p.Curtidas).WithOne().HasForeignKey(c => c.PostagemId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Postagem>()
                .HasMany(p => p.Comentarios).WithOne().HasForeignKey(c => c.PostagemId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Postagem>().HasIndex(p => p.DataCriacao);

            modelBuilder.Entity<Curtida>().HasIndex(c => new { c.PostagemId, c.UsuarioId }).IsUnique();
            modelBuilder.Entity<Curtida>()
                .HasOne<Usuario>().WithMany().HasForeignKey(c => c.UsuarioId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comentario>()
                .HasOne(c => c.Autor).WithMany().HasForeignKey(c => c.AutorId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Amizade>()
                .HasOne(a => a.Solicitante).WithMany().HasForeignKey(a => a.SolicitanteId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Amizade>()
                .HasOne(a => a.Destinatario).WithMany().HasForeignKey(a => a.DestinatarioId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Amizade>().HasIndex(a => new { a.SolicitanteId, a.DestinatarioId }).IsUnique();

            // ----- Chat -----
            modelBuilder.Entity<Conversa>()
                .HasOne(c => c.Iniciador).WithMany().HasForeignKey(c => c.IniciadorId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Conversa>()
                .HasOne(c => c.Destinatario).WithMany().HasForeignKey(c => c.DestinatarioId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Conversa>()
                .HasOne(c => c.Pet).WithMany().HasForeignKey(c => c.PetId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Conversa>()
                .HasOne(c => c.ItemDoacao).WithMany().HasForeignKey(c => c.ItemDoacaoId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Conversa>()
                .HasMany(c => c.Mensagens).WithOne().HasForeignKey(m => m.ConversaId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Conversa>().HasIndex(c => c.IniciadorId);
            modelBuilder.Entity<Conversa>().HasIndex(c => c.DestinatarioId);

            modelBuilder.Entity<Mensagem>()
                .HasOne(m => m.TermoAdocao).WithMany().HasForeignKey(m => m.TermoAdocaoId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Mensagem>()
                .HasOne<Usuario>().WithMany().HasForeignKey(m => m.RemetenteId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Mensagem>().HasIndex(m => new { m.ConversaId, m.DataEnvio });

            modelBuilder.Entity<TermoAdocao>()
                .HasOne(t => t.Pet).WithMany().HasForeignKey(t => t.PetId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TermoAdocao>()
                .HasOne(t => t.Doador).WithMany().HasForeignKey(t => t.DoadorId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TermoAdocao>()
                .HasOne(t => t.Adotante).WithMany().HasForeignKey(t => t.AdotanteId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TermoAdocao>()
                .HasOne<Conversa>().WithMany().HasForeignKey(t => t.ConversaId).OnDelete(DeleteBehavior.Cascade);

            // ----- Final Feliz -----
            modelBuilder.Entity<Depoimento>()
                .HasOne(d => d.Autor).WithMany().HasForeignKey(d => d.AutorId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Depoimento>()
                .HasOne(d => d.Pet).WithMany().HasForeignKey(d => d.PetId).OnDelete(DeleteBehavior.SetNull);

            // ----- Notificações -----
            modelBuilder.Entity<Notificacao>()
                .HasOne<Usuario>().WithMany().HasForeignKey(n => n.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Notificacao>().HasIndex(n => new { n.UsuarioId, n.Lida, n.DataCriacao });
            modelBuilder.Entity<Notificacao>().HasIndex(n => new { n.UsuarioId, n.Chave });

            // ----- Moderação e favoritos -----
            modelBuilder.Entity<Denuncia>()
                .HasOne(d => d.Denunciante).WithMany().HasForeignKey(d => d.DenuncianteId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Denuncia>().HasIndex(d => new { d.Status, d.DataCriacao });
            modelBuilder.Entity<Denuncia>().HasIndex(d => new { d.Tipo, d.AlvoId });

            modelBuilder.Entity<Favorito>()
                .HasOne<Usuario>().WithMany().HasForeignKey(f => f.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Favorito>()
                .HasOne(f => f.Pet).WithMany().HasForeignKey(f => f.PetId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Favorito>().HasIndex(f => new { f.UsuarioId, f.PetId }).IsUnique();

            modelBuilder.Entity<PerfilAdotante>()
                .HasOne<Usuario>().WithMany().HasForeignKey(p => p.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PerfilAdotante>().HasIndex(p => p.UsuarioId).IsUnique();

            modelBuilder.Entity<AvaliacaoUso>()
                .HasOne<Usuario>().WithMany().HasForeignKey(a => a.UsuarioId).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Midia>()
                .HasOne<Usuario>().WithMany().HasForeignKey(m => m.UsuarioId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
