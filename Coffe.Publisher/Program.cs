using Azure.Messaging.ServiceBus;
using Config;
using Domain;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;


ServiceBusClient sbClient = new ServiceBusClient(Settings.ConnectionString);

string Sentance = "Estamos ao vivo no canal do Henrique Eduardo Souza";

WriteLine("Coffe Publisher Console - Pressione <ENTER>", ConsoleColor.White);
Console.ReadLine();

//await SendTextString(Sentance);

//await SendControlMessageAsync();

await SendCoffeOrderAsync();

await SendCoffeOrderListAsMessagesAsync();

await SendCoffeOrderListAsBatchAsync();

WriteLine("Coffe Publisher Console - Finalizado. Pressione <ENTER>", ConsoleColor.White);
Console.ReadLine();

#region Metodos

async Task SendTextString(string text)
{
    WriteLine("Metodo: SendTextString", ConsoleColor.Cyan);    
    var sender = sbClient.CreateSender(Settings.QueueName);

    Write("Enviando...", ConsoleColor.Green);
    
    var message = new ServiceBusMessage(text);
    await sender.SendMessageAsync(message);

    WriteLine("Pronto!", ConsoleColor.Green);

    await sender.CloseAsync();
}

async Task SendTextStringAsMessagesAsync(string text)
{
    WriteLine("Metodo: SendTextStringAsMessagesAsync", ConsoleColor.Cyan);
    var sender = sbClient.CreateSender(Settings.QueueName);

    Write("Enviando:", ConsoleColor.Green);

    foreach (var letter in text.ToCharArray())
    {        
        var message = new ServiceBusMessage();
        message.Subject = letter.ToString();
        
        await sender.SendMessageAsync(message);
        Write(message.Subject, ConsoleColor.Green);
    }
    Console.WriteLine();
    
    await sender.CloseAsync();
}

async Task SendTextStringAsBatchAsync(string text)
{
    WriteLine("Metodo: SendTextStringAsBatchAsync", ConsoleColor.Cyan);
        
    var sender = sbClient.CreateSender(Settings.QueueName);

    Write("Enviando:", ConsoleColor.Green);
    var taskList = new List<Task>();

    var messageList = new List<ServiceBusMessage>();

    foreach (var letter in text.ToCharArray())
    {        
        var message = new ServiceBusMessage();
        message.Subject = letter.ToString();

        messageList.Add(message);
    }
    await sender.SendMessagesAsync(messageList);

    Console.WriteLine();
    
    await sender.CloseAsync();
}

async Task SendControlMessageAsync()
{
    WriteLine("Metodo: SendControlMessageAsync", ConsoleColor.Cyan);
        
    var message = new ServiceBusMessage()
    {
        Subject = "Control"
    };

    // Add some properties to the property collection
    message.ApplicationProperties.Add("SystemId", 1462);
    message.ApplicationProperties.Add("Command", "Pending Restart");
    message.ApplicationProperties.Add("ActionTime", DateTime.UtcNow.AddHours(2));
        
    var sender = sbClient.CreateSender(Settings.QueueName);
    Write("Enviando Control Message...", ConsoleColor.Green);
    await sender.SendMessageAsync(message);
    WriteLine("Pronto!", ConsoleColor.Green);
    Console.WriteLine();
    await sender.CloseAsync();
}

async Task SendCoffeOrderAsync()
{
    WriteLine("Metodo: SendCoffeOrderAsync", ConsoleColor.Cyan);

    var order = new CoffeOrder()
    {
        CustomerName = "Vinicius Climaco",
        Type = "Expresso",
        Size = "Grande"
    };
    
    var jsonCoffeOrder = JsonConvert.SerializeObject(order);
        
    var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonCoffeOrder))
    {
        Subject = "CoffeOrder",
        ContentType = "application/json"
    };
        
    var sender = sbClient.CreateSender(Settings.QueueName);
    Write("Enviando pedido...", ConsoleColor.Green);
    await sender.SendMessageAsync(message);
    WriteLine("Pronto!", ConsoleColor.Green);
    Console.WriteLine();
    await sender.CloseAsync();
}

async Task SendCoffeOrderListAsMessagesAsync()
{
    WriteLine("Metodo: SendCoffeOrderListAsMessagesAsync", ConsoleColor.Cyan);

    var coffeOrderList = GetCoffeOrderList();    
    var sender = sbClient.CreateSender(Settings.QueueName);

    WriteLine("Enviando...", ConsoleColor.Yellow);
    var watch = Stopwatch.StartNew();

    foreach (var coffeOrder in coffeOrderList)
    {
        var jsonCoffeOrder = JsonConvert.SerializeObject(coffeOrder);
        var message = new ServiceBusMessage(jsonCoffeOrder)
        {
            Subject = "CoffeOrder",
            ContentType = "application/json"
        };
        await sender.SendMessageAsync(message);
    }

    WriteLine($"Enviado { coffeOrderList.Count } pedidos! - Tempo: { watch.ElapsedMilliseconds } millisegundos, isto é { coffeOrderList.Count / watch.Elapsed.TotalSeconds } mensagens por segundo.", ConsoleColor.Green);
    Console.WriteLine();
    Console.WriteLine();
}

async Task SendCoffeOrderListAsBatchAsync()
{
    WriteLine("Metodo: SendCoffeOrderListAsBatchAsync", ConsoleColor.Cyan);

    var coffeOrderList = GetCoffeOrderList();
    var sender = sbClient.CreateSender(Settings.QueueName);

    var watch = Stopwatch.StartNew();
    using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

    foreach (var coffeOrder in coffeOrderList)
    {
        var jsonCoffeOrder = JsonConvert.SerializeObject(coffeOrder);
        var message = new ServiceBusMessage(jsonCoffeOrder)
        {
            Subject = "CoffeOrder",
            ContentType = "application/json"
        };
        if (!messageBatch.TryAddMessage(message))
            throw new Exception("A mensagem é muito grande para caber no batch");
    }

    WriteLine("Enviando...", ConsoleColor.Yellow);
    await sender.SendMessagesAsync(messageBatch);

    WriteLine($"Enviado { coffeOrderList.Count } pedidos! - Tempo: { watch.ElapsedMilliseconds } milisegundos, isto é { coffeOrderList.Count / watch.Elapsed.TotalSeconds } mensagens por segundo.", ConsoleColor.Green);
    Console.WriteLine();
    Console.WriteLine();
        
    await sender.CloseAsync();
}

List<CoffeOrder> GetCoffeOrderList()
{    
    string[] names = { "Vinicius", "Henrique", "Miguel", "Erica", "Maria", "Joao", "Jose", "Alex", "Eduardo", "Souza" };
    string[] coffes = { "Espresso", "Cappuccino", "Caffe latte", "Duplo", "Mocha", "Machiato", "Chocottino", "Americano", "Italiano", "Coado", "Panna", "Tradicional" };
    var listSize = new List<string> { "Pequeno", "Medio", "Grande" };
    var random = new Random();

    var coffeOrderList = new List<CoffeOrder>();
    for (int coffe = 0; coffe < coffes.Length; coffe++)
    {
        for (int name = 0; name < names.Length; name++)
        {
            int index = random.Next(listSize.Count);
            CoffeOrder order = new CoffeOrder()
            {
                CustomerName = names[name],
                Type = coffes[coffe],
                Size = listSize[index]
            };
            coffeOrderList.Add(order);
        }
    }
    return coffeOrderList;
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

#endregion

