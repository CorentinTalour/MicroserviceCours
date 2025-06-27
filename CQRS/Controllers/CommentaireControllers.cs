using CQRS.Data;
using CQRS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CQRS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentairesController : ControllerBase
{
    private readonly ReadDbContext _context;

    public CommentairesController(ReadDbContext context)
    {
        _context = context;
    }

    // GET: api/Commentaires
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommentaireReadModel>>> GetAllCommentaires()
    {
        var commentaires = await _context.Commentaires.ToListAsync();
        return Ok(commentaires);
    }

    // GET: api/Commentaires/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<CommentaireReadModel>> GetCommentaireById(int id)
    {
        var commentaire = await _context.Commentaires.FindAsync(id);

        if (commentaire == null)
            return NotFound();

        return Ok(commentaire);
    }

    // GET: api/Commentaires/byProduit/{produitId}
    [HttpGet("byProduit/{produitId}")]
    public async Task<ActionResult<IEnumerable<CommentaireReadModel>>> GetCommentairesByProduit(int produitId)
    {
        var commentaires = await _context.Commentaires
            .Where(c => c.ProduitId == produitId)
            .ToListAsync();

        return Ok(commentaires);
    }
}