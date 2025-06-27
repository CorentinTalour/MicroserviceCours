namespace CQRS.Event;

public class ProduitCreatedEvent
{
    public int Id { get; set; }
    public string Nom { get; set; }
    public decimal Prix { get; set; }
    public bool Notable { get; set; }
}