using System.Text.Json;
using CommentaireService.Data;
using CommentaireService.Dtos;
using CommentaireService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.CircuitBreaker;

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
        _httpClient = httpClientFactory.CreateClient("produit-service");
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

    // GET: api/commentaires/byProduit/{produitId} avec Fallback sécurisé
    [HttpGet("byProduit/{produitId}")]
    public async Task<ActionResult<object>> GetCommentairesByProduit(int produitId)
    {
        var fallbackPolicy = Policy<ProduitApiResponse?>
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<BrokenCircuitException>()
            .Or<JsonException>()
            .FallbackAsync(
                fallbackAction: (delegateResult, context, cancellationToken) =>
                {
                    Console.WriteLine($"Fallback déclenché pour GetCommentairesByProduit {produitId}: {delegateResult.Exception?.Message}");
                    return Task.FromResult<ProduitApiResponse?>(null);
                },
                onFallbackAsync: (delegateResult, context) =>
                {
                    Console.WriteLine($"Fallback activé pour GetCommentairesByProduit {produitId}");
                    return Task.CompletedTask;
                });

        var produitApiResponse = await fallbackPolicy.ExecuteAsync(async () =>
        {
            var response = await _httpClient.GetAsync($"/api/Produits/{produitId}");

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Erreur HTTP: {response.StatusCode}");
            }

            if (!response.Content.Headers.ContentType?.MediaType.Contains("application/json") == true)
            {
                throw new JsonException("Contenu non JSON retourné.");
            }

            var produitJson = await response.Content.ReadAsStringAsync();

            try
            {
                return JsonSerializer.Deserialize<ProduitApiResponse>(produitJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException)
            {
                throw new JsonException($"Erreur de parsing JSON. Contenu reçu : {produitJson}");
            }
        });

        var commentaires = await _context.Commentaires
            .Where(c => c.ProduitId == produitId)
            .ToListAsync();

        if (produitApiResponse == null || produitApiResponse.Produit == null)
        {
            return Ok(new
            {
                Produit = (object?)null,
                Commentaires = commentaires,
            });
        }

        return Ok(new
        {
            Produit = produitApiResponse.Produit,
            Commentaires = commentaires
        });
    }

    // POST: api/commentaires/{produitId} avec Fallback sécurisé
    [HttpPost("{produitId}")]
    public async Task<IActionResult> CreateCommentaire(int produitId, CreateCommentaireDto dto)
    {
        var fallbackPolicy = Policy<ProduitApiResponse?>
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<BrokenCircuitException>()
            .Or<JsonException>()
            .FallbackAsync(
                fallbackAction: (delegateResult, context, cancellationToken) =>
                {
                    Console.WriteLine($"⚡ Fallback déclenché pour CreateCommentaire {produitId}: {delegateResult.Exception?.Message}");
                    return Task.FromResult<ProduitApiResponse?>(null);
                },
                onFallbackAsync: (delegateResult, context) =>
                {
                    Console.WriteLine($"🚨 Fallback activé pour CreateCommentaire {produitId}");
                    return Task.CompletedTask;
                });

        var produitApiResponse = await fallbackPolicy.ExecuteAsync(async () =>
        {
            var response = await _httpClient.GetAsync($"/api/Produits/{produitId}");

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Erreur HTTP: {response.StatusCode}");
            }

            if (!response.Content.Headers.ContentType?.MediaType.Contains("application/json") == true)
            {
                throw new JsonException("Contenu non JSON retourné.");
            }

            var produitJson = await response.Content.ReadAsStringAsync();

            try
            {
                return JsonSerializer.Deserialize<ProduitApiResponse>(produitJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException)
            {
                throw new JsonException($"Erreur de parsing JSON. Contenu reçu : {produitJson}");
            }
        });

        if (produitApiResponse == null || produitApiResponse.Produit == null)
            return BadRequest("Service produit indisponible ou produit introuvable.");

        if (!produitApiResponse.Produit.Notable)
            return BadRequest("Impossible d'ajouter un commentaire à un produit non notable.");

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
