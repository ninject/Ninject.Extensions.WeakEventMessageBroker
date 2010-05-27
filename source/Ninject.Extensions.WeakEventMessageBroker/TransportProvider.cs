#region License

// 
// Author: Ian Davis <ian@innovatian.com>
// Copyright (c) 2009-2010, Innovatian Software, LLC
//
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
// See the file LICENSE.txt for details.
// 

#endregion

#region Using Directives

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
#if !NO_LCG
using System.Reflection.Emit;
#endif // !NO_LCG

#endregion

namespace Ninject.Extensions.WeakEventMessageBroker
{
    #region Using Directives

    using TransportTable = Dictionary<MethodInfo, Action<WeakReference, object, EventArgs>>;

    #endregion

    internal static class TransportProvider
    {
        private static readonly MethodInfo GetTarget = typeof (WeakReference).GetMethod( "get_Target" );

        private static readonly Type[] Parameters = {
                                                        typeof (WeakReference), typeof (object), typeof (EventArgs)
                                                    };

        private static readonly TransportTable Transports = new TransportTable();

#if SILVERLIGHT || NETCF
        private static readonly object SyncRoot = new object();
#else
        private static readonly ReaderWriterLockSlim TransportLock = new ReaderWriterLockSlim();
#endif

        internal static Action<WeakReference, object, EventArgs> GetTransport(MethodInfo method, DeliveryThread deliveryThread)
        {
#if SILVERLIGHT || NETCF
            lock (SyncRoot)
            {
                Action<WeakReference, object, EventArgs> existingTransport;
                if ( Transports.TryGetValue( method, out existingTransport ) )
                {
                    return existingTransport;
                }
                GuardAgainstClosures( method );
                Action<WeakReference, object, EventArgs> transport = CreateTransportMethod( method, deliveryThread );
                Transports[method] = transport;
                return transport;
            }
#else
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
                    Action<WeakReference, object, EventArgs> transport = CreateTransportMethod( method, deliveryThread );
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
#endif // SILVERLIGHT || NETCF
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

        private static Action<WeakReference, object, EventArgs> CreateTransportMethod( MethodInfo method, DeliveryThread deliveryThread )
        {
#if NO_LCG
            return ReflectionTransportGenerator( method, deliveryThread );
#else
            return DynamicTransportGenerator( method, deliveryThread );
#endif
        }

#if !NO_LCG
        internal static Action<WeakReference, object, EventArgs> DynamicTransportGenerator(MethodInfo method, DeliveryThread deliveryThread)
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
#endif // !NO_LCG

        internal static Action<WeakReference, object, EventArgs> ReflectionTransportGenerator(MethodInfo method, DeliveryThread deliveryThread)
        {
            SynchronizationContext context = null;
#if !SILVERLIGHT && !NETCF
            if (deliveryThread == DeliveryThread.UserInterface)
            {
                context = SynchronizationContext.Current;
            }
#endif
            return ( weakReference, sender, args ) =>
                   {
                       object target = null;
                       if ( !method.IsStatic )
                       {
                           target = GetTarget.Invoke( weakReference, null );
                           if ( target == null )
                           {
                               return;
                           }
                       }
                       Deliver(method, target, deliveryThread, sender, args, context);
                       
                   };
        }

        private static void Deliver(MethodInfo method, object target, DeliveryThread deliveryThread, object sender, EventArgs args, SynchronizationContext context)
        {
            switch (deliveryThread)
            {
                case DeliveryThread.Background:
                    DeliverViaBackgroundThread(method, target, sender, args);
                    break;
#if !SILVERLIGHT && !NETCF
                case DeliveryThread.UserInterface:
                    DeliverViaSynchronizationContext(method, context, target, sender, args);
                    break;
#endif
                default:
                    DeliverMessage(method, target, sender, args);
                    break;
            }
        }

        private static string GetAnonymousMethodName()
        {
            return "DynamicEventHandler" + Guid.NewGuid().ToString( "N" );
        }

        private static void DeliverMessage(MethodInfo method, object target, object sender, object args)
        {
            try
            {
                method.Invoke(target, new[] { sender, args });
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }
            }
        }

#if !SILVERLIGHT && !NETCF
        private static void DeliverViaSynchronizationContext(MethodInfo method, SynchronizationContext context, object target, object sender, object args)
        {
            if (context != null)
            {
                context.Send(delegate
                {
                    DeliverMessage(method, target, sender, args);
                }, null);
            }
            else
            {
                DeliverMessage(method, target, sender, args);
            }
        }
#endif

        private static void DeliverViaBackgroundThread(MethodInfo method, object target, object sender, object args)
        {
            ThreadPool.QueueUserWorkItem(s => DeliverMessage(method, target, sender, args));
        }
    }
}