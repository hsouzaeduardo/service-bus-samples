using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using DeadLetter.Config;
using Newtonsoft.Json;

ServiceBusSender ForwardingSender;

Utils.WriteLine("Consumer Console", ConsoleColor.White);
Console.WriteLine();

await CreateQueue();

var client = new ServiceBusClient(Settings.ConnectionString);
ForwardingSender = client.CreateSender(Settings.ForwardingQueueName);

await ReceiveMessages();


async Task CreateQueue()
{
    var administrationClient = new ServiceBusAdministrationClient(Settings.ConnectionString);
    if (!await administrationClient.QueueExistsAsync(Settings.QueueName))
    {
        await administrationClient.CreateQueueAsync(new CreateQueueOptions(Settings.QueueName)
        {
            LockDuration = TimeSpan.FromSeconds(5)
        });
    }
    if (!await administrationClient.QueueExistsAsync(Settings.ForwardingQueueName))
        await administrationClient.CreateQueueAsync(Settings.ForwardingQueueName);
}

async Task ReceiveMessages()
{
    var client = new ServiceBusClient(Settings.ConnectionString);
    var options = new ServiceBusProcessorOptions
    {        
        MaxConcurrentCalls = 1,
        AutoCompleteMessages = false
    };

    var processor = client.CreateProcessor(Settings.QueueName, options);
    processor.ProcessMessageAsync += ProcessMessage;
    processor.ProcessErrorAsync += ProcessError;

    await processor.StartProcessingAsync();
    Utils.WriteLine("Recebendo mensagens", ConsoleColor.Cyan);
    Console.ReadLine();

    await processor.StopProcessingAsync();
    await processor.CloseAsync();
}

async Task ProcessMessage(ProcessMessageEventArgs args)
{
    Utils.WriteLine("Recebida: " + args.Message.ContentType, ConsoleColor.Cyan);

    switch (args.Message.ContentType)
    {
        case "text/plain":
            await ProcessTextMessage(args);
            break;
        case "application/json":
            await ProcessJsonMessage(args);
            break;
        default:
            Console.WriteLine("Mensagem desconhecida recebida: " + args.Message.ContentType);
            await args.DeadLetterMessageAsync(args.Message, "Unknown message type", "The message type: " + args.Message.ContentType + " is not known.");
            break;
    }
}

async Task ProcessJsonMessage(ProcessMessageEventArgs args)
{
    var body = args.Message.Body.ToString();
    Utils.WriteLine($"JSON Message {args.Message.ToString()} " + body, ConsoleColor.Green);

    try
    {
        dynamic data = JsonConvert.DeserializeObject(body);
        Utils.WriteLine($"     Name: {data.contact.name}", ConsoleColor.Green);
        Utils.WriteLine($"     Twitter: {data.contact.twitter}", ConsoleColor.Green);

        await args.CompleteMessageAsync(args.Message);
        Utils.WriteLine("Mensagem processada", ConsoleColor.Cyan);
    }
    catch (Exception ex)
    {
        Utils.WriteLine($"Exception: { ex.Message }", ConsoleColor.Yellow);
        //await args.DeadLetterMessageAsync(args.Message, ex.Message, ex.ToString());
    }
}

async Task ProcessTextMessage(ProcessMessageEventArgs args)
{
    var body = args.Message.Body.ToString();
    Utils.WriteLine($"Mensagem: {body} - DeliveryCount: {args.Message.DeliveryCount}", ConsoleColor.Cyan);

    try
    {
        var forwardingMessage = new ServiceBusMessage();
        await ForwardingSender.SendMessageAsync(forwardingMessage);

        await args.CompleteMessageAsync(args.Message);

        Utils.WriteLine("Mensagem processada", ConsoleColor.Cyan);
    }
    catch (Exception ex)
    {
        Utils.WriteLine($"Exception: { ex.Message }", ConsoleColor.Yellow);
        //await args.AbandonMessageAsync(args.Message);
        if (args.Message.DeliveryCount > 5)
            await args.DeadLetterMessageAsync(args.Message, ex.Message, ex.ToString());
        else
            await args.AbandonMessageAsync(args.Message);
    }
}

async Task ProcessError(ProcessErrorEventArgs args)
{
    Utils.WriteLine($"Exception: { args.Exception.Message }", ConsoleColor.Yellow);
}