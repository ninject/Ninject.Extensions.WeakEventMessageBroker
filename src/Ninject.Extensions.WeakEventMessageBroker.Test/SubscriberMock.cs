#region Using Directives



#endregion

using System.Threading;

namespace Ninject.Extensions.WeakEventMessageBroker.Tests
{
    public class SubscriberMock
    {
        public string LastMessage { get; set; }

        [Subscribe( "message://PublisherMock/MessageReceived" )]
        public void OnMessageReceived( object sender, MessageEventArgs args )
        {
            LastMessage = args.Message;
        }
    }

    public class SubscriberBackgroundMock
    {
        public string LastMessage { get; set; }
        public int DeliveryThreadId { get; set; }
        public bool WasDeliveredFromThreadPool { get; set; }

        [Subscribe("message://PublisherMock/MessageReceived", Thread = DeliveryThread.Background)]
        public void OnMessageReceived(object sender, MessageEventArgs args)
        {
            LastMessage = args.Message;
            DeliveryThreadId = Thread.CurrentThread.ManagedThreadId;

#if !SILVERLIGHT
            WasDeliveredFromThreadPool = Thread.CurrentThread.IsThreadPoolThread;
#else
            WasDeliveredFromThreadPool = Thread.CurrentThread.IsBackground;
#endif
        }
    }

    public class SubscriberUserInterfaceMock
    {
        public string LastMessage { get; set; }
        public int DeliveryThreadId { get; set; }

        [Subscribe("message://PublisherMock/MessageReceived", Thread = DeliveryThread.UserInterface)]
        public void OnMessageReceived(object sender, MessageEventArgs args)
        {
            LastMessage = args.Message;
            DeliveryThreadId = Thread.CurrentThread.ManagedThreadId;
        }
    }
}