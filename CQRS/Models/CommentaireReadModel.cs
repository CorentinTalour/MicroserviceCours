namespace CQRS.Models;

public class CommentaireReadModel
{
    public int Id { get; set; }
    public string Texte { get; set; } = string.Empty;
    public double Note { get; set; }
    public int ProduitId { get; set; }

    // Propriété de navigation inverse
    public ProduitReadModel? Produit { get; set; }
}