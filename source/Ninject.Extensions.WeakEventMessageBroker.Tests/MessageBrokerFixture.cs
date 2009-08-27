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
                var publisher = kernel.Get<PublisherMock>();
                Assert.NotNull( publisher );

                var sub = kernel.Get<SubscriberMock>();
                Assert.NotNull( sub );

                Assert.True( publisher.HasListeners );
                Assert.Null( sub.LastMessage );

                publisher.SendMessage( "Hello, world!" );

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

                var subscriber = kernel.Get<SubscriberMock>();
                Assert.NotNull( subscriber );

                Assert.True( pub1.HasListeners );
                Assert.True( pub2.HasListeners );
                Assert.Null( subscriber.LastMessage );

                pub1.SendMessage( "Hello, world!" );
                Assert.Equal( subscriber.LastMessage, "Hello, world!" );

                subscriber.LastMessage = null;
                Assert.Null( subscriber.LastMessage );

                pub2.SendMessage( "Hello, world!" );
                Assert.Equal( subscriber.LastMessage, "Hello, world!" );
            }
        }

        [Fact]
        public void OnePublisherManySubscribers()
        {
            using ( var kernel = new StandardKernel() )
            {
                var publisher = kernel.Get<PublisherMock>();
                Assert.NotNull( publisher );

                var sub1 = kernel.Get<SubscriberMock>();
                var sub2 = kernel.Get<SubscriberMock>();
                Assert.NotNull( sub1 );
                Assert.NotNull( sub2 );

                Assert.True( publisher.HasListeners );
                Assert.Null( sub1.LastMessage );
                Assert.Null( sub2.LastMessage );

                publisher.SendMessage( "Hello, world!" );
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
                var publisher = kernel.Get<PublisherMock>();
                Assert.NotNull( publisher );

                var subscriber = kernel.Get<SubscriberMock>();
                Assert.NotNull( subscriber );

                Assert.Null( subscriber.LastMessage );

                var messageBroker = kernel.Components.Get<IWeakEventMessageBroker>();
                messageBroker.DisableChannel( "message://PublisherMock/MessageReceived" );
                Assert.True( publisher.HasListeners );

                publisher.SendMessage( "Hello, world!" );
                Assert.Null( subscriber.LastMessage );
            }
        }

        [Fact]
        public void ClosingChannelUnbindsPublisherEventsFromChannel()
        {
            using ( var kernel = new StandardKernel() )
            {
                var publisher = kernel.Get<PublisherMock>();
                Assert.NotNull( publisher );

                var subscriber = kernel.Get<SubscriberMock>();
                Assert.NotNull( subscriber );

                Assert.Null( subscriber.LastMessage );

                var messageBroker = kernel.Components.Get<IWeakEventMessageBroker>();
                messageBroker.CloseChannel( "message://PublisherMock/MessageReceived" );
                Assert.False( publisher.HasListeners );

                publisher.SendMessage( "Hello, world!" );
                Assert.Null( subscriber.LastMessage );
            }
        }

        [Fact]
        public void DisposingObjectRemovesSubscriptionsRequestedByIt()
        {
            using ( var kernel = new StandardKernel() )
            {
                var publisher = kernel.Get<PublisherMock>();
                Assert.NotNull( publisher );

                var subscriber = kernel.Get<SubscriberMock>();
                Assert.NotNull( subscriber );

                var messageBroker = kernel.Components.Get<IWeakEventMessageBroker>();
                IMessageChannel channel = messageBroker.GetChannel( "message://PublisherMock/MessageReceived" );
                Assert.Equal( channel.Subscriptions.Count, 1 );

                // Destroy the subscriber.
// ReSharper disable RedundantAssignment
                subscriber = null; // needed for GC to clean.
// ReSharper restore RedundantAssignment
                GC.Collect(2);
                GC.WaitForPendingFinalizers();

                // when messages are sent the subscriptions are updated.
                publisher.SendMessage( "message" );
                Assert.Equal( channel.Subscriptions.Count, 0 );
            }
        }

        [Fact]
        public void PublishersAreClearedWhenBrokerIsShutDown()
        {
            PublisherMock publisher;
            using ( var kernel = new StandardKernel() )
            {
                publisher = kernel.Get<PublisherMock>();
                Assert.NotNull( publisher );

                var sub = kernel.Get<SubscriberMock>();
                Assert.NotNull( sub );

                Assert.True( publisher.HasListeners );
            }
            Assert.False( publisher.HasListeners );
        }
    }
}