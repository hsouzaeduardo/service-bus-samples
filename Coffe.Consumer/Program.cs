using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Config;
using Domain;
using Newtonsoft.Json;
using System.Text;

ServiceBusClient sbClient;
sbClient = new ServiceBusClient(Settings.ConnectionString);

WriteLine("Coffe Consumer Console", ConsoleColor.White);

//await RecreateQueueAsync();

//await ReceiveAndProcessText(1);

await ReceiveAndProcessCoffeOrder(1);
//await ReceiveAndProcessCoffeOrder(5);
//await ReceiveAndProcessCoffeOrder(100);

//await ReceiveAndProcessControlMessage(1);

//await ReceiveAndProcessCharacters(1);

//await ReceiveAndProcessCharacters(16);

WriteLine("Recebendo, pressione <ENTER> para sair", ConsoleColor.White);
Console.ReadLine();


async Task ReceiveAndProcessText(int threads)
{
    WriteLine($"ReceiveAndProcessText({ threads })", ConsoleColor.Cyan);
    var options = new ServiceBusProcessorOptions()
    {
        AutoCompleteMessages = false,
        MaxConcurrentCalls = threads,
        MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(30)
    };

    var processor = sbClient.CreateProcessor(Settings.QueueName, options);

    processor.ProcessMessageAsync += ProcessTextMessageAsync;
    processor.ProcessErrorAsync += ErrorHandler;

    await processor.StartProcessingAsync();

    WriteLine("Receiving, hit enter to exit", ConsoleColor.White);
    Console.ReadLine();

    await processor.StopProcessingAsync();
    await processor.CloseAsync();
}

async Task ReceiveAndProcessControlMessage(int threads)
{
    WriteLine($"ReceiveAndProcessCoffeOrder({ threads })", ConsoleColor.Cyan);
    // Create a new client
    var options = new ServiceBusProcessorOptions
    {
        AutoCompleteMessages = false,
        MaxConcurrentCalls = threads,
        MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(10)
    };

    var processor = sbClient.CreateProcessor(Settings.QueueName, options);
    processor.ProcessMessageAsync += ProcessControlMessageAsync;
    processor.ProcessErrorAsync += ErrorHandler;

    await processor.StartProcessingAsync();

    WriteLine("Receiving, hit enter to exit", ConsoleColor.White);
    Console.ReadLine();

    await processor.StopProcessingAsync();
    await processor.CloseAsync();
}

async Task ReceiveAndProcessCoffeOrder(int threads)
{
    WriteLine($"ReceiveAndProcessCoffeOrder({ threads })", ConsoleColor.Cyan);
    var options = new ServiceBusProcessorOptions
    {
        AutoCompleteMessages = false,
        MaxConcurrentCalls = threads,
        MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(10)
    };

    var processor = sbClient.CreateProcessor(Settings.QueueName, options);
    processor.ProcessMessageAsync += ProcessCoffeMessageAsync;
    processor.ProcessErrorAsync += ErrorHandler;

    await processor.StartProcessingAsync();

    WriteLine("Receiving, hit enter to exit", ConsoleColor.White);
    Console.ReadLine();

    await processor.StopProcessingAsync();
    await processor.CloseAsync();
}

async Task ErrorHandler(ProcessErrorEventArgs args)
{
    WriteLine(args.Exception.Message, ConsoleColor.Red);
}

async Task ProcessCoffeMessageAsync(ProcessMessageEventArgs args)
{
    var coffeOrder = JsonConvert.DeserializeObject<CoffeOrder>(args.Message.Body.ToString());

    CookCoffe(coffeOrder);

    await args.CompleteMessageAsync(args.Message);
}

async Task ProcessTextMessageAsync(ProcessMessageEventArgs args)
{
    var messageBodyText = Encoding.UTF8.GetString(args.Message.Body);

    WriteLine($"Received: { messageBodyText }", ConsoleColor.Green);

    await args.CompleteMessageAsync(args.Message);
}

async Task ProcessControlMessageAsync(ProcessMessageEventArgs args)
{
    WriteLine($"Received: { args.Message.Subject }", ConsoleColor.Green);

    WriteLine("User properties...", ConsoleColor.Yellow);
    foreach (var property in args.Message.ApplicationProperties)
    {
        WriteLine($"    { property.Key } - { property.Value }", ConsoleColor.Cyan);
    }

    // Complete the message
    await args.CompleteMessageAsync(args.Message);

}

async Task ReceiveAndProcessCharacters(int threads)
{
    WriteLine($"ReceiveAndProcessCharacters({ threads })", ConsoleColor.Cyan);

    var options = new ServiceBusProcessorOptions
    {
        AutoCompleteMessages = false,
        MaxConcurrentCalls = threads,
        MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(30)
    };

    var processor = sbClient.CreateProcessor(Settings.QueueName, options);
    processor.ProcessMessageAsync += ProcessCharacterMessageAsync;
    processor.ProcessErrorAsync += ErrorHandler;

    await processor.StartProcessingAsync();

    WriteLine("Receiving... hit enter to exit", ConsoleColor.White);
}

async Task ProcessCharacterMessageAsync(ProcessMessageEventArgs args)
{
    Write(args.Message.Subject, ConsoleColor.Green);
}

async Task RecreateQueueAsync()
{
    var serviceBusAdministrationClient = new ServiceBusAdministrationClient(Settings.ConnectionString);
    if (await serviceBusAdministrationClient.QueueExistsAsync(Settings.QueueName))
    {
        WriteLine($"Deleting queue: { Settings.QueueName }...", ConsoleColor.Magenta);
        await serviceBusAdministrationClient.DeleteQueueAsync(Settings.QueueName);
        WriteLine("Done!", ConsoleColor.Magenta);
    }

    WriteLine($"Creating queue: { Settings.QueueName }...", ConsoleColor.Magenta);
    await serviceBusAdministrationClient.CreateQueueAsync(Settings.QueueName);
    WriteLine("Done!", ConsoleColor.Magenta);
}

void CookCoffe(CoffeOrder order)
{
    WriteLine($"Cooking {  order.Type } for { order.CustomerName }.", ConsoleColor.Yellow);
    Thread.Sleep(5000);
    WriteLine($"    { order.Type } coffe for {  order.CustomerName } is ready!", ConsoleColor.Green);
}

void WriteLine(string text, ConsoleColor color)
{
    var tempColor = Console.ForegroundColor;
    Console.ForegroundColor = color;
    Console.WriteLine(text);
    Console.ForegroundColor = tempColor;
}

void Write(string text, ConsoleColor color)
{
    var tempColor = Console.ForegroundColor;
    Console.ForegroundColor = color;
    Console.Write(text);
    Console.ForegroundColor = tempColor;
}