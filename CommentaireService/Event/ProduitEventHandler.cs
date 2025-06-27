using CommentaireService.Data;
using Steeltoe.Messaging.RabbitMQ.Attributes;
using Steeltoe.Messaging.RabbitMQ.Core;

namespace CommentaireService.Event;

public class ProduitEventHandler
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitTemplate _rabbitTemplate;

    public ProduitEventHandler(IServiceScopeFactory scopeFactory, RabbitTemplate rabbitTemplate)
    {
        _scopeFactory = scopeFactory;
        _rabbitTemplate = rabbitTemplate;
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

                // 🔥 On publie un événement RabbitMQ pour chaque commentaire supprimé
                foreach (var commentaire in commentaires)
                {
                    var deleteEvent = new CommentaireDeletedEvent
                    {
                        Id = commentaire.Id,
                        ProduitId = commentaire.ProduitId
                    };

                    _rabbitTemplate.ConvertAndSend("ms.commentaire", "commentaire.deleted", deleteEvent);
                    Console.WriteLine($"📤 Event commentaire.deleted envoyé pour le commentaire {commentaire.Id}");
                }
            }
            else
            {
                Console.WriteLine($"Aucun commentaire trouvé pour le produit {message.Id}");
            }
        }
    }
}