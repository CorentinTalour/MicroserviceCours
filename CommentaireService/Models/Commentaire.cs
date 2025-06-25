namespace CommentaireService.Models;

public class Commentaire
{
    public int Id { get; set; }
    public string Texte { get; set; }
    public int QualiteProduit { get; set; }
    public int RapportQualitePrix { get; set; }
    public int FaculteUtilisation { get; set; }
    public double Note { get; set; }
    public int ProduitId { get; set; }
}