namespace WebApplication1.Event;

public class ProduitUpdatedEvent
{
    public int Id { get; set; }
    public string Nom { get; set; }
    public decimal Prix { get; set; }
    public bool Notable { get; set; }
    public string Source { get; set; }
}