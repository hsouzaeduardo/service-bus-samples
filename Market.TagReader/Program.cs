using Azure.Messaging.ServiceBus;
using Market.Config;
using Market.Tag;
using Newtonsoft.Json;

Console.WriteLine("TagReader Console");

var serviceBusClient = new ServiceBusClient(Setting.ConnectionString);
var messageSender = serviceBusClient.CreateSender(Setting.QueueName);

// Lista de Pedidos
Tag[] orderItems = new Tag[]
{
                    new Tag() { Produto = "Teclado", Preco = 10.00 },
                    new Tag() { Produto = "Mouse", Preco = 5.00 },
                    new Tag() { Produto = "Microfone", Preco = 15.00 },
                    new Tag() { Produto = "Microfone", Preco = 15.00 },
                    new Tag() { Produto = "Camera", Preco = 100.00 },
                    new Tag() { Produto = "Camera", Preco = 100.00 },
                    new Tag() { Produto = "Celular", Preco = 800.00 },
                    new Tag() { Produto = "Celular", Preco = 800.00 },
                    new Tag() { Produto = "Fone-Ouvido", Preco = 200.00 },
                    new Tag() { Produto = "Fone-Ouvido", Preco = 200.00 },
};

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Pedido Contem {0} itens.", orderItems.Length);
Console.ForegroundColor = ConsoleColor.Yellow;


double orderTotal = 0.0;
foreach (Tag tag in orderItems)
{
    Console.WriteLine("{0} - ${1}", tag.Produto, tag.Preco);
    orderTotal += tag.Preco;
}
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Valor do Pedido = ${0}.", orderTotal);
Console.WriteLine();
Console.ResetColor();

Console.WriteLine("Pressione <ENTER> para scanear...");
Console.ReadLine();

Random random = new Random(DateTime.Now.Millisecond);

int sentCount = 0;
int position = 0;

Console.WriteLine("Lendo as tags...");
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Cyan;

// Comment in to create session id
//var sessionId = Guid.NewGuid().ToString();
//Console.WriteLine($"SessionId: { sessionId }");

while (position < 10)
{
    Tag tag = orderItems[position];

    var orderJson = JsonConvert.SerializeObject(tag);
    var tagReadMessage = new ServiceBusMessage(orderJson);

    // Comment in to set message id.
    //tagReadMessage.MessageId = tag.Id;

    // Comment in to set session id.
    //tagReadMessage.SessionId = sessionId;

    // Send the message
    await messageSender.SendMessageAsync(tagReadMessage);
    Console.WriteLine($"Enviado: { orderItems[position].Produto }");
    //Console.WriteLine($"Sent: { orderItems[position].Produto } - MessageId: { tagReadMessage.MessageId }");

    if (random.NextDouble() > 0.4) position++;
    sentCount++;

    Thread.Sleep(100);
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("{0} total tag´s lidas.", sentCount);
Console.WriteLine();
Console.ResetColor();

Console.ReadLine();