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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Ninject.Infrastructure.Disposal;

#endregion

namespace Ninject.Extensions.WeakEventMessageBroker
{
    /// <summary>
    /// 
    /// </summary>
    public class MessageChannel : DisposableObject, IMessageChannel
    {
        private static readonly MethodInfo BroadcastMethod = typeof (MessageChannel).GetMethod( "Broadcast",
                                                                                                new[]
                                                                                                {
                                                                                                    typeof (object),
                                                                                                    typeof (object)
                                                                                                } );

        private readonly List<Publication> _publications;
        private readonly List<TransportCacheEntry> _subscriptions;
        private volatile bool _enabled;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">The name of this channel.</param>
        public MessageChannel( string name )
        {
            Name = name;
            _subscriptions = new List<TransportCacheEntry>();
            _publications = new List<Publication>();
            _enabled = true;
        }

        #region Implementation of IMessageChannel

        /// <summary>
        /// Gets the name of the channel.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ICollection Subscriptions
        {
            get { return new List<TransportCacheEntry>( _subscriptions ).AsReadOnly(); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="eventInfo"></param>
        public void AddPublication( object instance, EventInfo eventInfo )
        {
            Delegate method = Delegate.CreateDelegate( eventInfo.EventHandlerType, this, BroadcastMethod );
            eventInfo.AddEventHandler( instance, method );
            lock ( _publications )
            {
                var publication = new Publication
                                  {
                                      Method = method,
                                      Instance = new WeakReference( instance ),
                                      Event = eventInfo
                                  };
                _publications.Add( publication );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        public void AddSubscription( object instance, MethodInfo method )
        {
            var transportCacheEntry = new TransportCacheEntry( TransportProvider.GetTransport( method ),
                                                               new WeakReference( instance ) );
            _subscriptions.Add( transportCacheEntry );
        }

        /// <summary>
        /// Closes the channel releasing its resources.
        /// </summary>
        public void Close()
        {
            lock ( _publications )
            {
                foreach ( Publication entry in _publications )
                {
                    WeakReference instance = entry.Instance;
                    if ( instance != null && instance.IsAlive )
                    {
                        entry.Event.RemoveEventHandler( instance.Target, entry.Method );
                    }
                }
                _publications.Clear();
            }
        }

        /// <summary>
        /// Enables the channel.
        /// </summary>
        public void Enable()
        {
            _enabled = true;
        }

        /// <summary>
        /// Disables the channel.
        /// </summary>
        public void Disable()
        {
            _enabled = false;
        }

        #endregion

        /// <summary>
        /// Releases resources held by the object.
        /// </summary>
        public override void Dispose( bool disposing )
        {
            if ( disposing && !IsDisposed )
            {
                Close();
            }
            base.Dispose( disposing );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void Broadcast( object sender, object args )
        {
            Raise( this, (EventArgs) args );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Raise( object sender, EventArgs e )
        {
            if ( !_enabled )
            {
                return;
            }

            lock ( _subscriptions )
            {
                foreach ( TransportCacheEntry transportCacheEntry in _subscriptions.ToArray() )
                {
                    transportCacheEntry.Transport( transportCacheEntry.Target, sender, e );
                }
            }
            RemoveDeadEntries();
        }

        // TODO: Can I tie this into the ninject lifestyle system?
        private void RemoveDeadEntries()
        {
            lock ( _subscriptions )
            {
                _subscriptions.RemoveAll( subscription => subscription.Target != null &&
                                                          !subscription.Target.IsAlive );
            }

            lock ( _publications )
            {
                _publications.RemoveAll( publication => publication.Instance != null &&
                                                        !publication.Instance.IsAlive );
            }
        }
    }
}