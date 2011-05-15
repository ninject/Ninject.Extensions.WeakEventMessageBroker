namespace Ninject.Extensions.WeakEventMessageBroker
{
    using System;
    using System.Diagnostics;
    using System.Threading;
#if !SILVERLIGHT
    using System.Windows.Forms;
#endif
    using Xunit;
    using Xunit.Should;

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

                publisher.HasListeners.ShouldBeTrue();
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

                sub.LastMessage.ShouldBe(Message);
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

                receivedMessage1.ShouldBe(Message1);
                receivedMessage2.ShouldBe(Message2);
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

                sub1.LastMessage.ShouldBe(Message);
                sub2.LastMessage.ShouldBe(Message);
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

                subscriber1ReceivedMessage1.ShouldBe(Message1);
                subscriber2ReceivedMessage1.ShouldBe(Message1);
                subscriber1ReceivedMessage2.ShouldBe(Message2);
                subscriber2ReceivedMessage2.ShouldBe(Message2);
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

                publisher.HasListeners.ShouldBeTrue();
                subscriber.LastMessage.ShouldBeNull();
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
                
                publisher.HasListeners.ShouldBeFalse();
                subscriber.LastMessage.ShouldBeNull();
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

                subscriber.ShouldNotBeNull();
                subscriber = null; // needed for GC to clean.
                GC.Collect();
                GC.WaitForPendingFinalizers();
                publisher.SendMessage("message"); // when messages are sent the subscriptions are updated.
                int subscriptionCountAfterSubscriberDisposal = channel.Subscriptions.Count;

                subscriptionCountBeforeSubscriberDisposal.ShouldBe(1);
                subscriptionCountAfterSubscriberDisposal.ShouldBe(0);
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
                sub.ShouldNotBeNull();
            }

            publisher.HasListeners.ShouldBeFalse();
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

                sub.LastMessage.ShouldBe(Message);
                sub.DeliveryThreadId.ShouldNotBe(0);
                sub.DeliveryThreadId.ShouldNotBe(id);
                sub.WasDeliveredFromThreadPool.ShouldBeTrue();
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
                sub.LastMessage.ShouldBe(UserInterfaceSynchronizedMessage);
                sub.DeliveryThreadId.ShouldBe(id);
#else
                subscriber = sub;
#endif
            }
        }

#if SILVERLIGHT
        [Fact]
        public void MessagesAreDeliveredOnTheUserInterfaceThread_Verify_Workaround()
        {
            subscriber.ShouldNotBeNull();

            var id = Thread.CurrentThread.ManagedThreadId;
            subscriber.LastMessage.ShouldBe(UserInterfaceSynchronizedMessage);
            subscriber.DeliveryThreadId.ShouldBe(id);
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