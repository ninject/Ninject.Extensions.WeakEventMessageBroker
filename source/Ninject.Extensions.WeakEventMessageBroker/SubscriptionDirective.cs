#region License

//
// Copyright © 2009 Ian Davis <ian.f.davis@gmail.com>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#endregion

#region Using Directives

using System.Reflection;
using Ninject.Planning.Directives;

#endregion

namespace Ninject.Extensions.WeakEventMessageBroker
{
    /// <summary>
    /// 
    /// </summary>
    public class SubscriptionDirective : IDirective
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionDirective"/> class.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="methodInfo">The method info.</param>
        public SubscriptionDirective( string channel, MethodInfo methodInfo )
        {
            Channel = channel;
            MethodInfo = methodInfo;
        }

        /// <summary>
        /// Gets the channel.
        /// </summary>
        /// <value>The channel.</value>
        public string Channel { get; private set; }

        /// <summary>
        /// Gets the method info used to call back on the subscriber when a
        /// message is received on the channel
        /// </summary>
        /// <value>The method info.</value>
        public MethodInfo MethodInfo { get; private set; }
    }
}