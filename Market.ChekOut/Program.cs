using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Market.Config;
using Market.Tag;
using Newtonsoft.Json;

Console.WriteLine("CheckOut Console");

var serviceBusAdministrationClient = new ServiceBusAdministrationClient(Setting.ConnectionString);

// Apagar a fila, caso exista
if (await serviceBusAdministrationClient.QueueExistsAsync(Setting.QueueName))
    await serviceBusAdministrationClient.DeleteQueueAsync(Setting.QueueName);

var rfidCreateQueueOptions = new CreateQueueOptions(Setting.QueueName)
{
    //RequiresDuplicateDetection = true,
    //DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(10),

    //RequiresSession = true
};

await serviceBusAdministrationClient.CreateQueueAsync(rfidCreateQueueOptions);

var serviceBusClient = new ServiceBusClient(Setting.ConnectionString);
var messageReceiver = serviceBusClient.CreateReceiver(Setting.QueueName);

Console.WriteLine("Recebendo tag...");
while (true)
{
    int receivedCount = 0;
    double billTotal = 0.0;
    Console.ForegroundColor = ConsoleColor.Cyan;

    //var sessionReceiver = await serviceBusClient.AcceptNextSessionAsync(Setting.QueueName);
    //Console.WriteLine("Sessão aceita: " + sessionReceiver.SessionId);

    Console.ForegroundColor = ConsoleColor.Yellow;

    while (true)
    {
        var receivedTagRead = await messageReceiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));
        //var receivedTadRead = await sessionReceiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));

        if (receivedTagRead != null)
        {
            var rfidJson = receivedTagRead.Body.ToString();
            var tag = JsonConvert.DeserializeObject<Tag>(rfidJson);
            Console.WriteLine("Compra de {0}", tag.Produto);
            receivedCount++;
            billTotal += tag.Preco;

            await messageReceiver.CompleteMessageAsync(receivedTagRead);
            //await sessionReceiver.CompleteMessageAsync(receivedTagRead);
        }
        else
        {
            //await sessionReceiver.CloseAsync();
            break;
        }
    }

    if (receivedCount > 0)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Compra Cliente ${0} por {1} itens.", billTotal, receivedCount);
        Console.WriteLine();
        Console.ResetColor();
    }
}