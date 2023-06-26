namespace DeadLetter.Config
{
    public class Settings
    {
        public static string ConnectionString = "Endpoint=sb://simplesmessaging.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=LKHwRGJLWuhgcX/Qx+vZXxyMSKt/Q9876cH6L5K9WAA=";
        public static string QueueName = "errorhandling";
        public static string ForwardingQueueName = "errorhandlingforwarding";
    }
}