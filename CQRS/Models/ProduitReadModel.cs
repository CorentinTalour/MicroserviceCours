namespace CQRS.Models;

public class ProduitReadModel
{
    public int Id { get; set; }
    public string Nom { get; set; }
    public decimal Prix { get; set; }
    public bool Notable { get; set; }
    public List<CommentaireReadModel> Commentaires { get; set; } = new();
}