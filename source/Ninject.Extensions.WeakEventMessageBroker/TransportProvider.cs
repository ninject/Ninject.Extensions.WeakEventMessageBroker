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
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;

#endregion

namespace Ninject.Extensions.WeakEventMessageBroker
{
    using TransportTable = Dictionary<MethodInfo, Action<WeakReference, object, EventArgs>>;

    internal static class TransportProvider
    {
        private static readonly MethodInfo GetTarget = typeof (WeakReference).GetMethod( "get_Target" );

        private static readonly Type[] Parameters = {
                                                        typeof (WeakReference), typeof (object), typeof (EventArgs)
                                                    };

        private static readonly ReaderWriterLockSlim TransportLock = new ReaderWriterLockSlim();
        private static readonly TransportTable Transports = new TransportTable();

        internal static Action<WeakReference, object, EventArgs> GetTransport( MethodInfo method )
        {
            TransportLock.EnterUpgradeableReadLock();
            try
            {
                Action<WeakReference, object, EventArgs> existingTransport;
                if ( Transports.TryGetValue( method, out existingTransport ) )
                {
                    return existingTransport;
                }
                TransportLock.EnterWriteLock();
                try
                {
                    GuardAgainstClosures( method );
                    Action<WeakReference, object, EventArgs> transport = CreateTransportMethod( method );
                    Transports[method] = transport;
                    return transport;
                }
                finally
                {
                    TransportLock.ExitWriteLock();
                }
            }
            finally
            {
                TransportLock.ExitUpgradeableReadLock();
            }
        }

        private static void GuardAgainstClosures( MethodInfo method )
        {
            Type generated = typeof (CompilerGeneratedAttribute);
            object[] attributes = method.DeclaringType.GetCustomAttributes( generated, false );
            if ( attributes.Length != 0 )
            {
                throw new InvalidOperationException( "Transport methods cannot be closures." );
            }
        }

        private static Action<WeakReference, object, EventArgs> CreateTransportMethod( MethodInfo method )
        {
#if SILVERLIGHT
			var dynamicMethod = new DynamicMethod( GetAnonymousMethodName(), null, Parameters, method.DeclaringType );
#else
            var dynamicMethod = new DynamicMethod( GetAnonymousMethodName(), null, Parameters, method.DeclaringType, true );
#endif

            ILGenerator il = dynamicMethod.GetILGenerator();
            if ( !method.IsStatic )
            {
                ILEmitter.EmitGuardCondition( il, method, GetTarget );
            }
            ILEmitter.EmitEventHandlerMethodCall( il, method, new[] {OpCodes.Ldarg_1, OpCodes.Ldarg_2} );
            ILEmitter.EmitReturn( il );

            return (Action<WeakReference, object, EventArgs>)
                   dynamicMethod.CreateDelegate( typeof (Action<WeakReference, object, EventArgs>) );
        }

        private static string GetAnonymousMethodName()
        {
            return "DynamicEventHandler" + Guid.NewGuid().ToString( "N" );
        }
    }
}