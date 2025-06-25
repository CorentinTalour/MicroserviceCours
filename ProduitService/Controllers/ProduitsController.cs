using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dtos;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProduitsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;

    public ProduitsController(AppDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient("commentaire-service");
    }

    // GET: api/produits
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Produit>>> GetProduits()
    {
        return await _context.Produits.ToListAsync();
    }
    
    // GET: api/produits/{id} avec commentaires
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

    // GET: api/produits/{id} avec commentaires
    [HttpGet("withComments/{id}")]
    public async Task<ActionResult<object>> GetProduitWithComments(int id)
    {
        var produit = await _context.Produits.FindAsync(id);

        if (produit == null)
            return NotFound();

        var response = await _httpClient.GetAsync($"api/commentaires/byProduit/{id}");

        List<CommentaireDto> commentaires = new();

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();

            var commentaireResponse = JsonSerializer.Deserialize<CommentaireResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (commentaireResponse != null)
                commentaires = commentaireResponse.Commentaires;
        }

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

        return NoContent();
    }

    // DELETE: api/produits/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduit(int id)
    {
        var produit = await _context.Produits.FindAsync(id);

        if (produit == null)
            return NotFound();

        var response = await _httpClient.GetAsync($"/api/commentaires/byProduit/{id}");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var commentaires = System.Text.Json.JsonSerializer.Deserialize<List<CommentaireDto>>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (commentaires != null && commentaires.Any())
                return BadRequest("Impossible de supprimer ce produit car des commentaires existent.");
        }

        _context.Produits.Remove(produit);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}