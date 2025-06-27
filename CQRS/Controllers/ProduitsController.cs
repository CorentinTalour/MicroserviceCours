using CQRS.Data;
using CQRS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CQRS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProduitsController : ControllerBase
{
    private readonly ReadDbContext _context;

    public ProduitsController(ReadDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProduitReadModel>>> GetAll()
    {
        var produits = await _context.Produits
            .Include(p => p.Commentaires)
            .ToListAsync();

        return Ok(produits);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProduitReadModel>> GetById(int id)
    {
        var produit = await _context.Produits
            .Include(p => p.Commentaires)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (produit == null)
            return NotFound();

        return Ok(produit);
    }
}