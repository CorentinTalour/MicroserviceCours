using CommentaireService.Models;
using Microsoft.EntityFrameworkCore;

namespace CommentaireService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Commentaire> Commentaires { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Commentaire>().ToTable("commentaires");
    }
}