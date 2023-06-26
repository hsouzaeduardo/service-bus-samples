using Azure.Messaging.ServiceBus;

string connectionString = "Endpoint=sb://sbhsouzaeastus001.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=NfF80FQ6Pud+ptXHxZ32dR0A8rG7btW2z+ASbD9M0YE=";
string queueName = "demo01";
string sentance = "Sejam todos bem vindos, a nossa demo de mensagem da Aula de Hoje do Henrique !!!!";

var client = new ServiceBusClient(connectionString);
var sender = client.CreateSender(queueName);

Console.WriteLine("Enviando Mensagens...");
foreach (var character in sentance)
{
    var message = new ServiceBusMessage(character.ToString());
    await sender.SendMessageAsync(message);
    Console.WriteLine($"    Enviado: { character }");

}

await sender.CloseAsync();

Console.WriteLine("Mensagens Enviadas...");
Console.ReadLine();