namespace CQRS.Models;

public class ProduitReadModel
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public decimal Prix { get; set; }
    public bool Notable { get; set; }

    // Propriété navigation collection
    public List<CommentaireReadModel> Commentaires { get; set; } = new();
}