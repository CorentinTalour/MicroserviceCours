using CommentaireService.Event;
using CQRS.Data;
using CQRS.Models;
using Microsoft.EntityFrameworkCore;
using Steeltoe.Messaging.RabbitMQ.Attributes;

namespace CQRS.Event;

public class CommentaireEventHandler
{
    private readonly IDbContextFactory<ReadDbContext> _contextFactory;

    public CommentaireEventHandler(IDbContextFactory<ReadDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
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

        using var context = _contextFactory.CreateDbContext();

        var commentaire = new CommentaireReadModel
        {
            Id = message.Id,
            Texte = message.Texte,
            Note = message.Note,
            ProduitId = message.ProduitId
        };

        context.Commentaires.Add(commentaire);
        context.SaveChanges();
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
        Console.WriteLine($"🗑️ Commentaire supprimé reçu : {message.Id}");

        using var context = _contextFactory.CreateDbContext();

        var commentaire = context.Commentaires.FirstOrDefault(c => c.Id == message.Id);
        if (commentaire != null)
        {
            context.Commentaires.Remove(commentaire);
            context.SaveChanges();
        }
    }
}