namespace CommentaireService.Dtos;

public class CreateCommentaireDto
{
    public string Texte { get; set; }
    public int QualiteProduit { get; set; }
    public int RapportQualitePrix { get; set; } 
    public int FaculteUtilisation { get; set; }
}