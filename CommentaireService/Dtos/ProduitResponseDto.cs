namespace CommentaireService.Dtos;

public class ProduitResponseDto
{
    public int Id { get; set; }
    public string Nom { get; set; }
    public decimal Prix { get; set; }
    public bool Notable { get; set; }

    public List<CommentaireResponseDto> Commentaires { get; set; }
}