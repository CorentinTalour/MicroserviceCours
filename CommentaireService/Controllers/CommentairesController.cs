using System.Text.Json;
using CommentaireService.Data;
using CommentaireService.Dtos;
using CommentaireService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommentaireService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentairesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;

    public CommentairesController(AppDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient("ProduitService");
    }

    // GET: api/commentaires
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Commentaire>>> GetCommentaires()
    {
        return await _context.Commentaires.ToListAsync();
    }

    // GET: api/commentaires/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Commentaire>> GetCommentaire(int id)
    {
        var commentaire = await _context.Commentaires.FindAsync(id);

        if (commentaire == null)
            return NotFound();

        return Ok(commentaire);
    }

    // GET: api/commentaires/byProduit/{produitId}
    [HttpGet("byProduit/{produitId}")]
    public async Task<ActionResult<IEnumerable<Commentaire>>> GetCommentairesByProduit(int produitId)
    {
        var commentaires = await _context.Commentaires
            .Where(c => c.ProduitId == produitId)
            .ToListAsync();

        return commentaires;
    }

    // POST: api/commentaires/{produitId}
    [HttpPost("{produitId}")]
    public async Task<IActionResult> CreateCommentaire(int produitId, CreateCommentaireDto dto)
    {
        var response = await _httpClient.GetAsync($"http://localhost:5185/api/produits/{produitId}");

        if (!response.IsSuccessStatusCode)
            return BadRequest("Produit introuvable.");

        var produitJson = await response.Content.ReadAsStringAsync();
        var produitApiResponse = System.Text.Json.JsonSerializer.Deserialize<ProduitApiResponse>(
            produitJson,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (produitApiResponse == null || produitApiResponse.Produit == null || !produitApiResponse.Produit.Notable)
            return BadRequest("Impossible d'ajouter un commentaire à un produit non notable.");

        // Calcul de la note moyenne
        double noteMoyenne = (dto.QualiteProduit + dto.RapportQualitePrix + dto.FaculteUtilisation) / 3.0;

        var commentaire = new Commentaire
        {
            Texte = dto.Texte,
            QualiteProduit = dto.QualiteProduit,
            RapportQualitePrix = dto.RapportQualitePrix,
            FaculteUtilisation = dto.FaculteUtilisation,
            Note = noteMoyenne,
            ProduitId = produitId
        };

        _context.Commentaires.Add(commentaire);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCommentairesByProduit), new { produitId = produitId }, commentaire);
    }

    // DELETE: api/commentaires/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCommentaire(int id)
    {
        var commentaire = await _context.Commentaires.FindAsync(id);

        if (commentaire == null)
            return NotFound();

        _context.Commentaires.Remove(commentaire);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}