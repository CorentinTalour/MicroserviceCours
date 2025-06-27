namespace CQRS.Event;

public class CommentaireDeletedEvent
{
    public int Id { get; set; }
    public int ProduitId { get; set; }
}