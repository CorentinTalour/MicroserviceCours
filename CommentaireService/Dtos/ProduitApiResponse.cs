namespace CommentaireService.Dtos;

public class ProduitApiResponse
{
    public ProduitDto Produit { get; set; }
    public List<CommentaireDto> Commentaires { get; set; }
}