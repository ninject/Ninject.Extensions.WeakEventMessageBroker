namespace Ninject.Extensions.WeakEventMessageBroker
{
    using System;
    using System.Diagnostics;
    using System.Threading;
#if !SILVERLIGHT
    using System.Windows.Forms;
#endif
    using FluentAssertions;

    using Xunit;

    public class MessageBrokerFixture
    {
        private const string UserInterfaceSynchronizedMessage = "test";
#if SILVERLIGHT
        private static SubscriberUserInterfaceMock subscriber;
#endif

        [Fact]
        public void PublisherHasAListener()
        {
            using (var kernel = this.CreateKernel())
            {
                var publisher = kernel.Get<PublisherMock>();

                publisher.HasListeners.Should().BeTrue();
            }
        }

        [Fact]
        public void OnePublisherOneSubscriber()
        {
            using (var kernel = this.CreateKernel())
            {
                const string Message = "Hello, world!";
                var publisher = kernel.Get<PublisherMock>();
                var sub = kernel.Get<SubscriberMock>();

                publisher.SendMessage(Message);

                sub.LastMessage.Should().Be(Message);
            }
        }

        [Fact]
        public void ManyPublishersOneSubscriber()
        {
            using (var kernel = this.CreateKernel())
            {
                const string Message1 = "Hello, world1!";
                const string Message2 = "Hello, world2!";
                var pub1 = kernel.Get<PublisherMock>();
                var pub2 = kernel.Get<PublisherMock>();
                var subscriber = kernel.Get<SubscriberMock>();

                pub1.SendMessage(Message1);
                var receivedMessage1 = subscriber.LastMessage;
                pub2.SendMessage(Message2);
                var receivedMessage2 = subscriber.LastMessage;

                receivedMessage1.Should().Be(Message1);
                receivedMessage2.Should().Be(Message2);
            }
        }

        [Fact]
        public void OnePublisherManySubscribers()
        {
            using (var kernel = this.CreateKernel())
            {
                const string Message = "Hello, world!";
                var publisher = kernel.Get<PublisherMock>();
                var sub1 = kernel.Get<SubscriberMock>();
                var sub2 = kernel.Get<SubscriberMock>();

                publisher.SendMessage(Message);

                sub1.LastMessage.Should().Be(Message);
                sub2.LastMessage.Should().Be(Message);
            }
        }

        [Fact]
        public void ManyPublishersManySubscribers()
        {
            using (var kernel = this.CreateKernel())
            {
                const string Message1 = "Hello, world1!";
                const string Message2 = "Hello, world2!";
                var pub1 = kernel.Get<PublisherMock>();
                var pub2 = kernel.Get<PublisherMock>();
                var sub1 = kernel.Get<SubscriberMock>();
                var sub2 = kernel.Get<SubscriberMock>();

                pub1.SendMessage(Message1);
                var subscriber1ReceivedMessage1 = sub1.LastMessage;
                var subscriber2ReceivedMessage1 = sub2.LastMessage;
                pub2.SendMessage(Message2);
                var subscriber1ReceivedMessage2 = sub1.LastMessage;
                var subscriber2ReceivedMessage2 = sub2.LastMessage;

                subscriber1ReceivedMessage1.Should().Be(Message1);
                subscriber2ReceivedMessage1.Should().Be(Message1);
                subscriber1ReceivedMessage2.Should().Be(Message2);
                subscriber2ReceivedMessage2.Should().Be(Message2);
            }
        }

        [Fact]
        public void DisabledChannelsDoNotUnbindButEventsAreNotSent()
        {
            using (var kernel = this.CreateKernel())
            {
                var publisher = kernel.Get<PublisherMock>();
                var subscriber = kernel.Get<SubscriberMock>();

                var messageBroker = kernel.Components.Get<IWeakEventMessageBroker>();
                messageBroker.DisableChannel("message://PublisherMock/MessageReceived");
                publisher.SendMessage("Hello, world!");

                publisher.HasListeners.Should().BeTrue();
                subscriber.LastMessage.Should().BeNull();
            }
        }

        [Fact]
        public void ClosingChannelUnbindsPublisherEventsFromChannel()
        {
            using (var kernel = this.CreateKernel())
            {
                var publisher = kernel.Get<PublisherMock>();
                var subscriber = kernel.Get<SubscriberMock>();

                var messageBroker = kernel.Components.Get<IWeakEventMessageBroker>();
                messageBroker.CloseChannel("message://PublisherMock/MessageReceived");
                publisher.SendMessage("Hello, world!");

                publisher.HasListeners.Should().BeFalse();
                subscriber.LastMessage.Should().BeNull();
            }
        }

        [Fact]
        public void DisposingObjectRemovesSubscriptionsRequestedByIt()
        {
            using (var kernel = this.CreateKernel())
            {
                var publisher = kernel.Get<PublisherMock>();
                var subscriber = kernel.Get<SubscriberMock>();

                var messageBroker = kernel.Components.Get<IWeakEventMessageBroker>();
                IMessageChannel channel = messageBroker.GetChannel("message://PublisherMock/MessageReceived");
                int subscriptionCountBeforeSubscriberDisposal = channel.Subscriptions.Count;

                subscriber.Should().NotBeNull();
                subscriber = null; // needed for GC to clean.
                GC.Collect();
                GC.WaitForPendingFinalizers();
                publisher.SendMessage("message"); // when messages are sent the subscriptions are updated.
                int subscriptionCountAfterSubscriberDisposal = channel.Subscriptions.Count;

                subscriptionCountBeforeSubscriberDisposal.Should().Be(1);
                subscriptionCountAfterSubscriberDisposal.Should().Be(0);
            }
        }

        [Fact]
        public void PublishersAreClearedWhenBrokerIsShutDown()
        {
            PublisherMock publisher;
            using (var kernel = this.CreateKernel())
            {
                publisher = kernel.Get<PublisherMock>();
                var sub = kernel.Get<SubscriberMock>();
                sub.Should().NotBeNull();
            }

            publisher.HasListeners.Should().BeFalse();
        }

        [Fact]
        public void MessagesAreDeliveredOnBackgroundThreads()
        {
            using (var kernel = this.CreateKernel())
            {
                const string Message = "test";
                var publisher = kernel.Get<PublisherMock>();
                var sub = kernel.Get<SubscriberBackgroundMock>();
                var id = Thread.CurrentThread.ManagedThreadId;

                publisher.SendMessage(Message);
                Thread.Sleep(100);

                sub.LastMessage.Should().Be(Message);
                sub.DeliveryThreadId.Should().NotBe(0);
                sub.DeliveryThreadId.Should().NotBe(id);
                sub.WasDeliveredFromThreadPool.Should().BeTrue();
            }
        }

        [Fact]
        public void MessagesAreDeliveredOnTheUserInterfaceThread()
        {
#if !SILVERLIGHT // In silverlight the tests are already running on the UI Thread with the Synchronization context set.
            SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
#endif

            using (var kernel = this.CreateKernel())
            {
                var publisher = kernel.Get<PublisherMock>();
                var sub = kernel.Get<SubscriberUserInterfaceMock>();
                var id = Thread.CurrentThread.ManagedThreadId;
                Debug.WriteLine("Current Thread : " + id);
                Debug.WriteLine("Context : " + SynchronizationContext.Current);

                ThreadPool.QueueUserWorkItem(s => publisher.SendMessage(UserInterfaceSynchronizedMessage));
                Thread.Sleep(1000); // give the BG thread enough time to be created and execute.
#if !SILVERLIGHT
                Application.DoEvents(); // processes all waiting messages
                sub.LastMessage.Should().Be(UserInterfaceSynchronizedMessage);
                sub.DeliveryThreadId.Should().Be(id);
#else
                subscriber = sub;
#endif
            }
        }

#if SILVERLIGHT
        [Fact]
        public void MessagesAreDeliveredOnTheUserInterfaceThread_Verify_Workaround()
        {
            subscriber.Should().NotBeNull();

            var id = Thread.CurrentThread.ManagedThreadId;
            subscriber.LastMessage.Should().Be(UserInterfaceSynchronizedMessage);
            subscriber.DeliveryThreadId.Should().Be(id);
        }
#endif

        private IKernel CreateKernel()
        {
#if !SILVERLIGHT
            return new StandardKernel();
#else
            return new StandardKernel(new WeakEventMessageBrokerModule());
#endif
        }
    }
}