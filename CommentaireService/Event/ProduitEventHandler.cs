using CommentaireService.Data;
using Microsoft.EntityFrameworkCore;
using Steeltoe.Messaging.RabbitMQ.Attributes;

namespace CommentaireService.Event;

public class ProduitEventHandler
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ProduitEventHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    [DeclareQueue(Name = "ms.produit.deletion.queue")]
    [DeclareQueueBinding(
        Name = "ProduitDeletedBinding",
        QueueName = "ms.produit.deletion.queue",
        ExchangeName = "ms.produit",
        RoutingKey = "produit.deleted")]
    [RabbitListener(Binding = "ProduitDeletedBinding")]
    public void On(ProduitDeletedEvent message)
    {
        Console.WriteLine($"Produit supprimé reçu : {message.Id}");

        using (var scope = _scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var commentaires = dbContext.Commentaires.Where(c => c.ProduitId == message.Id).ToList();

            if (commentaires.Any())
            {
                dbContext.Commentaires.RemoveRange(commentaires);
                dbContext.SaveChanges();

                Console.WriteLine($"Commentaires supprimés pour le produit {message.Id}");
            }
            else
            {
                Console.WriteLine($"Aucun commentaire trouvé pour le produit {message.Id}");
            }
        }
    }
}