using Steeltoe.Messaging.RabbitMQ.Attributes;

namespace CommentaireService.Event;

public class CommentaireEventHandler
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CommentaireEventHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    // Listener pour la création
    [DeclareQueue(Name = "readmodel.commentaire.creation.queue")]
    [DeclareQueueBinding(
        Name = "CommentaireCreatedBinding",
        QueueName = "readmodel.commentaire.creation.queue",
        ExchangeName = "ms.commentaire",
        RoutingKey = "commentaire.created")]
    [RabbitListener(Binding = "CommentaireCreatedBinding")]
    public void OnCommentaireCreated(CommentaireCreatedEvent message)
    {
        Console.WriteLine($"Commentaire créé reçu : {message.Id}");
    }

    // Listener pour la suppression
    [DeclareQueue(Name = "readmodel.commentaire.deletion.queue")]
    [DeclareQueueBinding(
        Name = "CommentaireDeletedBinding",
        QueueName = "readmodel.commentaire.deletion.queue",
        ExchangeName = "ms.commentaire",
        RoutingKey = "commentaire.deleted")]
    [RabbitListener(Binding = "CommentaireDeletedBinding")]
    public void OnCommentaireDeleted(CommentaireDeletedEvent message)
    {
        Console.WriteLine($"Commentaire supprimé reçu : {message.Id}");
    }
}