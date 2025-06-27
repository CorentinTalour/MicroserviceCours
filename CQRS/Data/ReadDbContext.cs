using CQRS.Models;
using Microsoft.EntityFrameworkCore;

namespace CQRS.Data;

public class ReadDbContext : DbContext
{
    public ReadDbContext(DbContextOptions<ReadDbContext> options) : base(options) {}

    public DbSet<ProduitReadModel> Produits { get; set; }
    public DbSet<CommentaireReadModel> Commentaires { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CommentaireReadModel>()
            .HasOne(c => c.Produit)
            .WithMany(p => p.Commentaires)
            .HasForeignKey(c => c.ProduitId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}