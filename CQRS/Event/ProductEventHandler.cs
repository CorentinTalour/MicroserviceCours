using CQRS.Data;
using CQRS.Models;
using Microsoft.EntityFrameworkCore;
using Steeltoe.Messaging.RabbitMQ.Attributes;

namespace CQRS.Event;

public class ProductEventHandler
{
    private readonly IDbContextFactory<ReadDbContext> _contextFactory;

    public ProductEventHandler(IDbContextFactory<ReadDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    
    [DeclareQueue(Name = "readmodel.produit.creation.queue")]
    [DeclareQueueBinding(Name = "ProduitCreatedBinding", QueueName = "readmodel.produit.creation.queue", ExchangeName = "ms.produit", RoutingKey = "produit.creation")]
    [RabbitListener(Binding = "ProduitCreatedBinding")]
    public void OnProduitCreated(ProduitCreatedEvent message)
    {
        Console.WriteLine($"[ReadModel] Produit créé reçu : {message.Nom}");

        using var context = _contextFactory.CreateDbContext();

        context.Produits.Add(new ProduitReadModel
        {
            Id = message.Id,
            Nom = message.Nom,
            Prix = message.Prix,
            Notable = message.Notable
        });

        context.SaveChanges();
    }
    
    [DeclareQueue(Name = "readmodel.produit.update.queue")]
    [DeclareQueueBinding(Name = "ProduitUpdatedBinding", QueueName = "readmodel.produit.update.queue", ExchangeName = "ms.produit", RoutingKey = "produit.updated")]
    [RabbitListener(Binding = "ProduitUpdatedBinding")]
    public void OnProduitUpdated(ProduitUpdatedEvent message)
    {
        Console.WriteLine($"[ReadModel] Produit mis à jour reçu : {message.Id}");

        using var context = _contextFactory.CreateDbContext();
        
        var produit = context.Produits.FirstOrDefault(p => p.Id == message.Id);
        if (produit != null)
        {
            produit.Nom = message.Nom;
            produit.Prix = message.Prix;
            produit.Notable = message.Notable;
            context.SaveChanges();
        }
    }

    [DeclareQueue(Name = "readmodel.produit.deletion.queue")]
    [DeclareQueueBinding(Name = "ProduitDeletedBinding", QueueName = "readmodel.produit.deletion.queue", ExchangeName = "ms.produit", RoutingKey = "produit.deleted")]
    [RabbitListener(Binding = "ProduitDeletedBinding")]
    public void OnProduitDeleted(ProduitDeletedEvent message)
    {
        Console.WriteLine($"[ReadModel] Produit supprimé reçu : {message.Id}");

        using var context = _contextFactory.CreateDbContext();

        var produit = context.Produits.FirstOrDefault(p => p.Id == message.Id);
        if (produit != null)
        {
            context.Produits.Remove(produit);
            context.SaveChanges();
        }
    }
}