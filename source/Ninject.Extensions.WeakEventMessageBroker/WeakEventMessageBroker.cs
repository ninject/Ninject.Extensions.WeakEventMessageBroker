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
using Ninject.Infrastructure;

#endregion

namespace Ninject.Extensions.WeakEventMessageBroker
{
    public class WeakEventMessageBroker : NinjectComponent, IWeakEventMessageBroker, IHaveKernel
    {
        private readonly Dictionary<string, IMessageChannel> _channels;

        public WeakEventMessageBroker( IKernel kernel )
        {
            Kernel = kernel;
            _channels = new Dictionary<string, IMessageChannel>();
        }

        #region Implementation of IWeakEventMessageBroker

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

        public void CloseChannel( string name )
        {
            IMessageChannel channel;
            lock ( _channels )
            {
                if ( _channels.ContainsKey( name ) )
                {
                    channel = _channels[name];
                    channel.Close();
                }
            }
        }

        public void EnableChannel( string name )
        {
            IMessageChannel channel = GetChannel( name );
            channel.Enable();
        }

        public void DisableChannel( string name )
        {
            IMessageChannel channel = GetChannel( name );
            channel.Disable();
        }

        #endregion

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

        #region Implementation of IHaveKernel

        public IKernel Kernel { get; private set; }

        #endregion
    }
}