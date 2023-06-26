using Azure.Messaging.ServiceBus;

string connectionString = "Endpoint=sb://sbhsouzaeastus001.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=NfF80FQ6Pud+ptXHxZ32dR0A8rG7btW2z+ASbD9M0YE=";
string queueName = "demo01";

var client = new ServiceBusClient(connectionString);
var receiver = client.CreateReceiver(queueName);

Console.WriteLine("Recebendo Mensagens...");

while (true)
{
    var message = await receiver.ReceiveMessageAsync();
    if (message != null)
    {
        string messageBody = message.Body.ToString();

        Console.Write(messageBody);

        if (!messageBody.Equals("H"))
        {
            await receiver.CompleteMessageAsync(message);
        }
    }
    else
    {
        Console.WriteLine();
        Console.WriteLine("Todas as mensagens foram recebidas.");
        break;
    }
}

await receiver.CloseAsync();