namespace WebApplication1.Dtos;

public class CommentaireResponse
{
    public ProduitDto Produit { get; set; }
    public List<CommentaireDto> Commentaires { get; set; }
}