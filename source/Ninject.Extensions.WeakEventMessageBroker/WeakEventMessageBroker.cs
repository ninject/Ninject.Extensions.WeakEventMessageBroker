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

using System.Collections.Generic;
using Ninject.Components;

#endregion

namespace Ninject.Extensions.WeakEventMessageBroker
{
    /// <summary>
    /// 
    /// </summary>
    public class WeakEventMessageBroker : NinjectComponent, IWeakEventMessageBroker
    {
        private readonly Dictionary<string, IMessageChannel> _channels;

        /// <summary>
        /// 
        /// </summary>
        public WeakEventMessageBroker()
        {
            _channels = new Dictionary<string, IMessageChannel>();
        }

        #region Implementation of IWeakEventMessageBroker

        /// <summary>
        /// Returns a channel with the specified name, creating it first if necessary.
        /// </summary>
        /// <param name="name">The name of the channel to create or retrieve.</param>
        /// <returns>The object representing the channel.</returns>
        public IMessageChannel GetChannel( string name )
        {
            IMessageChannel channel;
            lock ( _channels )
            {
                if ( _channels.ContainsKey( name ) )
                {
                    return _channels[name];
                }
                channel = new MessageChannel( name );
                _channels.Add( name, channel );
            }
            return channel;
        }

        /// <summary>
        /// Closes a channel, removing it from the message broker.
        /// </summary>
        /// <param name="name">The name of the channel to close.</param>
        public void CloseChannel( string name )
        {
            IMessageChannel channel;
            lock ( _channels )
            {
                if ( !_channels.ContainsKey( name ) )
                {
                    return;
                }
                channel = _channels[name];
                _channels.Remove( name );
                channel.Close();
            }
        }

        /// <summary>
        /// Enables a channel, causing it to pass messages through as they occur.
        /// </summary>
        /// <param name="name">The name of the channel to enable.</param>
        public void EnableChannel( string name )
        {
            IMessageChannel channel = GetChannel( name );
            channel.Enable();
        }

        /// <summary>
        /// Disables a channel, which will block messages from being passed.
        /// </summary>
        /// <param name="name">The name of the channel to disable.</param>
        public void DisableChannel( string name )
        {
            IMessageChannel channel = GetChannel( name );
            channel.Disable();
        }

        #endregion

        /// <summary>
        /// Releases resources held by the object.
        /// </summary>
        public override void Dispose( bool disposing )
        {
            if ( disposing && !IsDisposed )
            {
                lock ( _channels )
                {
                    foreach ( IMessageChannel channel in _channels.Values )
                    {
                        channel.Dispose();
                    }
                    _channels.Clear();
                }
            }
            base.Dispose( disposing );
        }
    }
}