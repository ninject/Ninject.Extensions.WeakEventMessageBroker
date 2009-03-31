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
    public class PublicationDirective : IDirective
    {
        private readonly string _channel;
        private readonly EventInfo _eventInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublicationDirective"/> class.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="eventInfo">The event tied to the channel.</param>
        public PublicationDirective( string channel, EventInfo eventInfo )
        {
            _channel = channel;
            _eventInfo = eventInfo;
        }

        /// <summary>
        /// Gets the channel.
        /// </summary>
        /// <value>The channel.</value>
        public string Channel
        {
            get { return _channel; }
        }


        /// <summary>
        /// Gets the event tied to the channel.
        /// </summary>
        /// <value>The event.</value>
        public EventInfo Event
        {
            get { return _eventInfo; }
        }
    }
}