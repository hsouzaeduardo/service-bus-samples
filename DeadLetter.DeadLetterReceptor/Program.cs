using Azure.Messaging.ServiceBus;
using DeadLetter.Config;

Utils.WriteLine("DeadLetterReceptor Console", ConsoleColor.White);
Console.WriteLine();

Thread.Sleep(3000);

var client = new ServiceBusClient(Settings.ConnectionString);
var processorOptions = new ServiceBusProcessorOptions
{
    MaxConcurrentCalls = 1,
    AutoCompleteMessages = false,
    SubQueue = SubQueue.DeadLetter
};
var processor = client.CreateProcessor(Settings.QueueName, processorOptions);

Utils.WriteLine($"Dead letter queue path {processor.EntityPath}", ConsoleColor.White);

processor.ProcessMessageAsync += ProcessDeadLetterMessageAsync;
processor.ProcessErrorAsync += ProcessErrorAsync;

await processor.StartProcessingAsync();

Utils.WriteLine("Recebendo mensagens da Dead-Letter", ConsoleColor.Cyan);
Console.WriteLine();

Console.ReadLine();
await processor.StopProcessingAsync();
await processor.CloseAsync();

async Task ProcessDeadLetterMessageAsync(ProcessMessageEventArgs args)
{
    Utils.WriteLine("Mensagem recebida da Dead-Letter", ConsoleColor.Cyan);
    Utils.WriteLine($"    Content type:   {args.Message.ContentType}", ConsoleColor.Green);
    Utils.WriteLine($"    DeadLetterReason:   {args.Message.DeadLetterReason}", ConsoleColor.Green);
    Utils.WriteLine($"    DeadLetterErrorDescription:   {args.Message.DeadLetterErrorDescription}", ConsoleColor.Green);

    await args.CompleteMessageAsync(args.Message);

    Console.WriteLine();
}

async Task ProcessErrorAsync(ProcessErrorEventArgs args)
{
    Utils.WriteLine($"Exception: { args.Exception.Message }", ConsoleColor.Yellow);
}