using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace fnFila
{
    public class Function1
    {
        [FunctionName("Function1")]
        public void Run([ServiceBusTrigger("demo01", Connection = "Endpoint=sb://sbhsouzaeastus001.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=NfF80FQ6Pud+ptXHxZ32dR0A8rG7btW2z+ASbD9M0YE=")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
        }
    }
}
