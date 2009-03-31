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
    public class MessageChannel : DisposableObject, IMessageChannel
    {
        private static readonly MethodInfo _broadcastMethod = typeof (MessageChannel).GetMethod( "Broadcast",
                                                                                                 new[]
                                                                                                 {
                                                                                                     typeof (object),
                                                                                                     typeof (object)
                                                                                                 } );

        private readonly List<Publication> _publications;
        private readonly List<EventEntry> _subscriptions;
        private bool _enabled;

        public MessageChannel( string name )
        {
            Name = name;
            _subscriptions = new List<EventEntry>();
            _publications = new List<Publication>();
            _enabled = true;
        }

        #region Implementation of IMessageChannel

        public string Name { get; private set; }

        public ICollection Subscriptions
        {
            get { return new List<EventEntry>( _subscriptions ).AsReadOnly(); }
        }

        public void AddPublication( object instance, EventInfo eventInfo )
        {
            Delegate method = Delegate.CreateDelegate( eventInfo.EventHandlerType, this, _broadcastMethod );
            eventInfo.AddEventHandler( instance, method );
            lock ( _publications )
            {
                _publications.Add( new Publication
                                   {Method = method, Instance = new WeakReference( instance ), Event = eventInfo} );
            }
        }

        public void AddSubscription( object instance, MethodInfo method )
        {
            _subscriptions.Add( new EventEntry( FastSmartWeakEventForwarderProvider.GetForwarder( method ), method,
                                                new WeakReference( instance ) ) );
        }

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

        public void Enable()
        {
            _enabled = true;
        }

        public void Disable()
        {
            _enabled = false;
        }

        #endregion

        public override void Dispose( bool disposing )
        {
            if ( disposing && !IsDisposed )
            {
                Close();
            }
            base.Dispose( disposing );
        }

        public void Broadcast( object sender, object args )
        {
            Raise( this, (EventArgs) args );
        }

        public void Raise( object sender, EventArgs e )
        {
            if ( !_enabled )
            {
                return;
            }

            lock ( _subscriptions )
            {
                foreach ( EventEntry ee in _subscriptions.ToArray() )
                {
                    ee.Forwarder( ee.TargetReference, sender, e );
                }
            }
            RemoveDeadEntries();
        }

        // TODO: Can I tie this into the ninject lifestyle system?
        private void RemoveDeadEntries()
        {
            lock ( _subscriptions )
            {
                _subscriptions.RemoveAll( subscription => subscription.TargetReference != null &&
                                                          !subscription.TargetReference.IsAlive );
            }

            lock ( _publications )
            {
                _publications.RemoveAll( publication => publication.Instance != null &&
                                                        !publication.Instance.IsAlive );
            }
        }
    }
}