#region Using Directives

using Ninject.Extensions.WeakEventMessageBroker;

#endregion

namespace Ninject.Extensions.WeakEventBroker.Tests
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
}