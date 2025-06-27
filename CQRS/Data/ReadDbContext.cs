using CQRS.Models;
using Microsoft.EntityFrameworkCore;

namespace CQRS.Data;

public class ReadDbContext: DbContext
{
    public ReadDbContext(DbContextOptions<ReadDbContext> options) : base(options) {}

    public DbSet<ProduitReadModel> Produits { get; set; }
    public DbSet<CommentaireReadModel> Commentaires { get; set; }
}