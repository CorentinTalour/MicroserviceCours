namespace CQRS.Event;

public class CommentaireCreatedEvent
{
    public int Id { get; set; }
    public string Texte { get; set; }
    public double Note { get; set; }
    public int ProduitId { get; set; }
}