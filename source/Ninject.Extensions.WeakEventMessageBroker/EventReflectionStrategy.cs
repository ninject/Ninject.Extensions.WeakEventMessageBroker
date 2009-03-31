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
using System.Reflection;
using Ninject.Components;
using Ninject.Infrastructure.Language;
using Ninject.Injection;
using Ninject.Planning;
using Ninject.Planning.Strategies;
using Ninject.Selection;

#endregion

namespace Ninject.Extensions.WeakEventMessageBroker
{
    /// <summary>
    /// A planning strategy that examines types via reflection to determine if there are any
    /// message publications or subscriptions defined.
    /// </summary>
    public class EventReflectionStrategy : NinjectComponent, IPlanningStrategy
    {
        private const BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public EventReflectionStrategy( ISelector selector, IInjectorFactory injectorFactory )
        {
            Selector = selector;
            InjectorFactory = injectorFactory;
        }

        #region Implementation of IPlanningStrategy

        /// <summary>
        /// Contributes to the specified plan.
        /// </summary>
        /// <param name="plan">The plan that is being generated.</param>
        public void Execute( IPlan plan )
        {
            EventInfo[] events = plan.Type.GetEvents( _bindingFlags );

            foreach ( EventInfo evt in events )
            {
                IEnumerable<PublishAttribute> attributes = evt.GetAttributes<PublishAttribute>();

                foreach ( PublishAttribute attribute in attributes )
                {
                    plan.Add( new PublicationDirective( attribute.Channel, evt ) );
                }
            }

            MethodInfo[] methods =
                plan.Type.GetMethods( _bindingFlags );

            foreach ( MethodInfo method in methods )
            {
                IEnumerable<SubscribeAttribute> attributes = method.GetAttributes<SubscribeAttribute>();

                foreach ( SubscribeAttribute attribute in attributes )
                {
                    plan.Add( new SubscriptionDirective( attribute.Channel, method ) );
                }
            }
        }

        #endregion

        /// <summary>
        /// Gets the selector component.
        /// </summary>
        public ISelector Selector { get; private set; }

        /// <summary>
        /// Gets the injector factory component.
        /// </summary>
        public IInjectorFactory InjectorFactory { get; set; }
    }
}