using Azure.Messaging.ServiceBus;
using DeadLetter.Config;

ServiceBusSender sender;

Console.WriteLine("Publisher Console");
Console.WriteLine();

Thread.Sleep(3000);

var client = new ServiceBusClient(Settings.ConnectionString);
sender = client.CreateSender(Settings.QueueName);

while (true)
{
    Console.WriteLine("Digite: text, json, poison, unknown ou exit");

    var messageType = Console.ReadLine().ToLower();
    if (messageType == "exit")
        break;

    switch (messageType)
    {
        case "text":
            await SendMessage("Olá! Seja Bem Vindo!", "text/plain");
            break;
        case "json":
            await SendMessage("{\"contact\": {\"name\": \"Vinicius\",\"twitter\": \"@vinicius.climaco\" }}", "application/json");
            break;
        case "poison":
            await SendMessage("<contact><name>Vinicius</name><twitter>@vinicius.climaco</twitter></contact>", "application/json");
            break;
        case "unknown":
            await SendMessage("Unknown message", "application/unknown");
            break;

        default:
            Console.WriteLine("O que você quis dizer?");
            break;
    }
}
await sender.CloseAsync();


async Task SendMessage(string text, string contentType)
{
    try
    {
        var message = new ServiceBusMessage(text);
        message.ContentType = contentType;
        Utils.WriteLine($"Mensagem Criada: { text }", ConsoleColor.Cyan);

        await sender.SendMessageAsync(message);
        Utils.WriteLine("Mensagem Enviada", ConsoleColor.Cyan);
    }
    catch (Exception ex)
    {
        Utils.WriteLine(ex.Message, ConsoleColor.Yellow);
    }
}
