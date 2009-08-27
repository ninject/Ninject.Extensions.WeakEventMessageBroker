#region License

//
// Author: Nate Kohari <nkohari@gmail.com>
// Copyright (c) 2007-2009, Enkari, Ltd.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
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
        public override void Activate( IContext context )
        {
            var messageBroker = context.Kernel.Components.Get<IWeakEventMessageBroker>();

            List<PublicationDirective> publications = context.Plan.GetAll<PublicationDirective>().ToList();

            foreach ( PublicationDirective publication in publications )
            {
                IMessageChannel channel = messageBroker.GetChannel( publication.Channel );
                channel.AddPublication( context.Instance, publication.Event );
            }

            List<SubscriptionDirective> subscriptions = context.Plan.GetAll<SubscriptionDirective>().ToList();

            foreach ( SubscriptionDirective subscription in subscriptions )
            {
                IMessageChannel channel = messageBroker.GetChannel( subscription.Channel );
                channel.AddSubscription( context.Instance, subscription.MethodInfo );
            }
        }
    }
}