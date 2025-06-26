using Steeltoe.Messaging.RabbitMQ.Attributes;
using WebApplication1.Models;

namespace WebApplication1.Event;

public class CustomEventHandler
{
    [DeclareQueue(Name = "ms.produit.creation.queue")]
    [DeclareQueueBinding(
        Name = "ProduitCreatedBinding",
        QueueName = "ms.produit.creation.queue",
        ExchangeName = "ms.produit",
        RoutingKey = "produit.creation")]
    [RabbitListener(Binding = "ProduitCreatedBinding")]
    public void On(ProduitCreatedEvent message)
    {
        Console.WriteLine($"Message reçu : {message.Nom} - {message.Prix}");
    }
}