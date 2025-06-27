using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.CircuitBreaker;
using Steeltoe.Messaging.RabbitMQ.Core;
using WebApplication1.Data;
using WebApplication1.Dtos;
using WebApplication1.Event;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProduitsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly RabbitTemplate _rabbitTemplate;

    public ProduitsController(AppDbContext context, IHttpClientFactory httpClientFactory, RabbitTemplate rabbitTemplate)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient("commentaire-service");
        _rabbitTemplate = rabbitTemplate;
    }

    // GET: api/produits
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Produit>>> GetProduits()
    {
        return await _context.Produits.ToListAsync();
    }

    // GET: api/produits/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetProduit(int id)
    {
        var produit = await _context.Produits.FindAsync(id);

        if (produit == null)
            return NotFound();

        return Ok(new
        {
            Produit = produit
        });
    }

    // GET: api/produits/withComments/{id} avec Fallback
    [HttpGet("withComments/{id}")]
    public async Task<ActionResult<object>> GetProduitWithComments(int id)
    {
        var produit = await _context.Produits.FindAsync(id);

        if (produit == null)
            return NotFound();

        // Fallback policy
        var fallbackPolicy = Policy<List<CommentaireDto>>
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<BrokenCircuitException>()
            .FallbackAsync(
                fallbackAction: (delegateResult, context, cancellationToken) =>
                {
                    Console.WriteLine($"Fallback déclenché pour le produit {id} : {delegateResult.Exception?.Message}");
                    return Task.FromResult(new List<CommentaireDto>());
                },
                onFallbackAsync: (delegateResult, context) =>
                {
                    Console.WriteLine($"⚡ Fallback activé pour le produit {id}");
                    return Task.CompletedTask;
                });

        var commentaires = await fallbackPolicy.ExecuteAsync(async () =>
        {
            var response = await _httpClient.GetAsync($"http://commentaire-service/api/commentaires/byProduit/{id}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var commentaireResponse = JsonSerializer.Deserialize<CommentaireResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return commentaireResponse?.Commentaires ?? new List<CommentaireDto>();
            }
            else
            {
                // Si le service retourne une erreur HTTP, on retourne une liste vide (sans déclencher le fallback)
                return new List<CommentaireDto>();
            }
        });

        return Ok(new
        {
            Produit = produit,
            Commentaires = commentaires
        });
    }

// POST: api/produits
    [HttpPost]
    public async Task<ActionResult<Produit>> CreateProduit(Produit produit)
    {
        _context.Produits.Add(produit);
        await _context.SaveChangesAsync();

        // Envoi de l'évènement RabbitMQ ici
        var message = new ProduitCreatedEvent
        {
            Id = produit.Id,
            Nom = produit.Nom,
            Prix = produit.Prix,
            Notable = produit.Notable,
            Source = "ProduitService"
        };

        _rabbitTemplate.ConvertAndSend("ms.produit", "produit.creation", message);
        
        return CreatedAtAction(nameof(GetProduit), new { id = produit.Id }, produit);
    }

    // PUT: api/produits/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduit(int id, Produit updatedProduit)
    {
        if (id != updatedProduit.Id)
            return BadRequest("L'identifiant du produit ne correspond pas.");

        var produit = await _context.Produits.FindAsync(id);

        if (produit == null)
            return NotFound();

        produit.Nom = updatedProduit.Nom;
        produit.Prix = updatedProduit.Prix;
        produit.Notable = updatedProduit.Notable;

        await _context.SaveChangesAsync();

        // Envoyer l'événement RabbitMQ
        var message = new ProduitUpdatedEvent
        {
            Id = produit.Id,
            Nom = produit.Nom,
            Prix = produit.Prix,
            Notable = produit.Notable,
            Source = "ProduitService"
        };

        _rabbitTemplate.ConvertAndSend("ms.produit", "produit.updated", message);

        return NoContent();
    }

    // DELETE: api/produits/{id}
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteProduit(int id)
{
    var produit = await _context.Produits.FindAsync(id);

    if (produit == null)
        return NotFound();

    // Fallback policy avec meilleure gestion des erreurs
    var fallbackPolicy = Policy<List<CommentaireDto>>
        .Handle<HttpRequestException>()
        .Or<TaskCanceledException>()
        .Or<BrokenCircuitException>()
        .Or<JsonException>()
        .FallbackAsync(
            fallbackAction: (delegateResult, context, cancellationToken) =>
            {
                Console.WriteLine($"Fallback déclenché (DELETE) pour le produit {id} : {delegateResult.Exception?.Message}");
                return Task.FromResult(new List<CommentaireDto>());
            },
            onFallbackAsync: (delegateResult, context) =>
            {
                Console.WriteLine($"⚡ Fallback activé (DELETE) pour le produit {id}");
                return Task.CompletedTask;
            });

    var commentaires = await fallbackPolicy.ExecuteAsync(async () =>
    {
        try
        {
            var response = await _httpClient.GetAsync($"http://commentaire-service/api/commentaires/byProduit/{id}");

            // Vérifier d'abord si le statut est OK
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Service commentaire a répondu avec statut {response.StatusCode}");
                return new List<CommentaireDto>();
            }
            
            // Vérifier que le content-type est bien du JSON
            var contentType = response.Content.Headers.ContentType?.MediaType;
            if (contentType == null || !contentType.Contains("application/json"))
            {
                Console.WriteLine($"La réponse n'est pas en JSON: {contentType}");
                return new List<CommentaireDto>();
            }

            var json = await response.Content.ReadAsStringAsync();
            
            // Vérifier que le JSON n'est pas vide
            if (string.IsNullOrWhiteSpace(json))
            {
                Console.WriteLine("La réponse JSON est vide");
                return new List<CommentaireDto>();
            }
            
            var commentaireResponse = JsonSerializer.Deserialize<CommentaireResponse>(json, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return commentaireResponse?.Commentaires ?? new List<CommentaireDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'appel au service commentaire: {ex.Message}");
            throw;
        }
    });

    _context.Produits.Remove(produit);
    await _context.SaveChangesAsync();
    
    // Envoi du message RabbitMQ pour la suppression du produit
    var message = new ProduitDeletedEvent { Id = produit.Id };
    _rabbitTemplate.ConvertAndSend("ms.produit", "produit.deleted", message);
    
    return NoContent();
    }
}