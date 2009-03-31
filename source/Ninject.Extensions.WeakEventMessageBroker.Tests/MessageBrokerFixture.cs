#region Using Directives

using System;
using Xunit;

#endregion

namespace Ninject.Extensions.WeakEventMessageBroker.Tests
{
    public class MessageBrokerFixture
    {
        [Fact]
        public void OnePublisherOneSubscriber()
        {
            using ( var kernel = new StandardKernel() )
            {
                var pub = kernel.Get<PublisherMock>();
                Assert.NotNull( pub );

                var sub = kernel.Get<SubscriberMock>();
                Assert.NotNull( sub );

                Assert.True( pub.HasListeners );
                Assert.Null( sub.LastMessage );

                pub.SendMessage( "Hello, world!" );

                Assert.Equal( sub.LastMessage, "Hello, world!" );
            }
        }

        [Fact]
        public void ManyPublishersOneSubscriber()
        {
            using ( var kernel = new StandardKernel() )
            {
                var pub1 = kernel.Get<PublisherMock>();
                var pub2 = kernel.Get<PublisherMock>();
                Assert.NotNull( pub1 );
                Assert.NotNull( pub2 );

                var sub = kernel.Get<SubscriberMock>();
                Assert.NotNull( sub );

                Assert.True( pub1.HasListeners );
                Assert.True( pub2.HasListeners );
                Assert.Null( sub.LastMessage );

                pub1.SendMessage( "Hello, world!" );
                Assert.Equal( sub.LastMessage, "Hello, world!" );

                sub.LastMessage = null;
                Assert.Null( sub.LastMessage );

                pub2.SendMessage( "Hello, world!" );
                Assert.Equal( sub.LastMessage, "Hello, world!" );
            }
        }

        [Fact]
        public void OnePublisherManySubscribers()
        {
            using ( var kernel = new StandardKernel() )
            {
                var pub = kernel.Get<PublisherMock>();
                Assert.NotNull( pub );

                var sub1 = kernel.Get<SubscriberMock>();
                var sub2 = kernel.Get<SubscriberMock>();
                Assert.NotNull( sub1 );
                Assert.NotNull( sub2 );

                Assert.True( pub.HasListeners );
                Assert.Null( sub1.LastMessage );
                Assert.Null( sub2.LastMessage );

                pub.SendMessage( "Hello, world!" );
                Assert.Equal( sub1.LastMessage, "Hello, world!" );
                Assert.Equal( sub2.LastMessage, "Hello, world!" );
            }
        }

        [Fact]
        public void ManyPublishersManySubscribers()
        {
            using ( var kernel = new StandardKernel() )
            {
                var pub1 = kernel.Get<PublisherMock>();
                var pub2 = kernel.Get<PublisherMock>();
                Assert.NotNull( pub1 );
                Assert.NotNull( pub2 );

                var sub1 = kernel.Get<SubscriberMock>();
                var sub2 = kernel.Get<SubscriberMock>();
                Assert.NotNull( sub1 );
                Assert.NotNull( sub2 );

                Assert.True( pub1.HasListeners );
                Assert.True( pub2.HasListeners );
                Assert.Null( sub1.LastMessage );
                Assert.Null( sub2.LastMessage );

                pub1.SendMessage( "Hello, world!" );
                Assert.Equal( sub1.LastMessage, "Hello, world!" );
                Assert.Equal( sub2.LastMessage, "Hello, world!" );

                sub1.LastMessage = null;
                sub2.LastMessage = null;
                Assert.Null( sub1.LastMessage );
                Assert.Null( sub2.LastMessage );

                pub2.SendMessage( "Hello, world!" );
                Assert.Equal( sub1.LastMessage, "Hello, world!" );
                Assert.Equal( sub2.LastMessage, "Hello, world!" );
            }
        }

        [Fact]
        public void DisabledChannelsDoNotUnbindButEventsAreNotSent()
        {
            using ( var kernel = new StandardKernel() )
            {
                var pub = kernel.Get<PublisherMock>();
                Assert.NotNull( pub );

                var sub = kernel.Get<SubscriberMock>();
                Assert.NotNull( sub );

                Assert.Null( sub.LastMessage );

                var messageBroker = kernel.Components.Get<IWeakEventMessageBroker>();
                messageBroker.DisableChannel( "message://PublisherMock/MessageReceived" );
                Assert.True( pub.HasListeners );

                pub.SendMessage( "Hello, world!" );
                Assert.Null( sub.LastMessage );
            }
        }

        [Fact]
        public void ClosingChannelUnbindsPublisherEventsFromChannel()
        {
            using ( var kernel = new StandardKernel() )
            {
                var pub = kernel.Get<PublisherMock>();
                Assert.NotNull( pub );

                var sub = kernel.Get<SubscriberMock>();
                Assert.NotNull( sub );

                Assert.Null( sub.LastMessage );

                var messageBroker = kernel.Components.Get<IWeakEventMessageBroker>();
                messageBroker.CloseChannel( "message://PublisherMock/MessageReceived" );
                Assert.False( pub.HasListeners );

                pub.SendMessage( "Hello, world!" );
                Assert.Null( sub.LastMessage );
            }
        }

        [Fact]
        public void DisposingObjectRemovesSubscriptionsRequestedByIt()
        {
            using ( var kernel = new StandardKernel() )
            {
                var pub = kernel.Get<PublisherMock>();
                Assert.NotNull( pub );

                var sub = kernel.Get<SubscriberMock>();
                Assert.NotNull( sub );

                var messageBroker = kernel.Components.Get<IWeakEventMessageBroker>();
                IMessageChannel channel = messageBroker.GetChannel( "message://PublisherMock/MessageReceived" );
                Assert.Equal( channel.Subscriptions.Count, 1 );

                // Destroy the subscriber.
// ReSharper disable RedundantAssignment
                sub = null; // needed for GC to clean.
// ReSharper restore RedundantAssignment
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // when messages are sent the subscriptions are updated.
                pub.SendMessage( "message" );
                Assert.Equal( channel.Subscriptions.Count, 0 );
                Assert.Empty( channel.Subscriptions );
            }
        }

        [Fact]
        public void PublishersAreClearedWhenBrokerIsShutDown()
        {
            PublisherMock pub;
            using ( var kernel = new StandardKernel() )
            {
                pub = kernel.Get<PublisherMock>();
                Assert.NotNull( pub );

                var sub = kernel.Get<SubscriberMock>();
                Assert.NotNull( sub );

                Assert.True( pub.HasListeners );
            }
            Assert.False( pub.HasListeners );
        }
    }
}