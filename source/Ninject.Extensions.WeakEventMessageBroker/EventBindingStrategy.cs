#region License

// 
// Author: Nate Kohari <nate@enkari.com>
// Copyright (c) 2007-2010, Enkari, Ltd.
// 
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
// See the file LICENSE.txt for details.
// 

#endregion

#region Using Directives

using System.Collections.Generic;
using System.Linq;
using Ninject.Activation;
using Ninject.Activation.Strategies;

#endregion

namespace Ninject.Extensions.WeakEventMessageBroker
{
    /// <summary>
    /// An activation strategy that binds events to message channels after instances are initialized
    /// </summary>
    public class EventBindingStrategy : ActivationStrategy
    {
        /// <summary>
        /// Contributes to the activation of the instance in the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="reference">The instance to process.</param>
        public override void Activate( IContext context, InstanceReference reference )
        {
            var messageBroker = context.Kernel.Components.Get<IWeakEventMessageBroker>();

            List<PublicationDirective> publications = context.Plan.GetAll<PublicationDirective>().ToList();

            foreach ( PublicationDirective publication in publications )
            {
                IMessageChannel channel = messageBroker.GetChannel( publication.Channel );
                channel.AddPublication( reference.Instance, publication.Event );
            }

            List<SubscriptionDirective> subscriptions = context.Plan.GetAll<SubscriptionDirective>().ToList();

            foreach ( SubscriptionDirective subscription in subscriptions )
            {
                IMessageChannel channel = messageBroker.GetChannel( subscription.Channel );
                channel.AddSubscription( reference.Instance, subscription.MethodInfo );
            }
        }
    }
}