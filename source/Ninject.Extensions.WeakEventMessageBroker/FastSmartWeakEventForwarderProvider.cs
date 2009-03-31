#region License

// Copyright (c) 2008 Daniel Grunwald
// 
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

#endregion

#region Using Directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

#endregion

namespace Ninject.Extensions.WeakEventMessageBroker
{
    internal static class FastSmartWeakEventForwarderProvider
    {
        private static readonly Type[] forwarderParameters = {
                                                                 typeof (WeakReference), typeof (object),
                                                                 typeof (EventArgs)
                                                             };

        private static readonly Dictionary<MethodInfo, ForwarderDelegate> forwarders =
            new Dictionary<MethodInfo, ForwarderDelegate>();

        private static readonly MethodInfo getTarget = typeof (WeakReference).GetMethod( "get_Target" );

        internal static ForwarderDelegate GetForwarder( MethodInfo method )
        {
            lock ( forwarders )
            {
                ForwarderDelegate d;
                if ( forwarders.TryGetValue( method, out d ) )
                {
                    return d;
                }
            }

            if ( method.DeclaringType.GetCustomAttributes( typeof (CompilerGeneratedAttribute), false ).Length != 0 )
            {
                throw new ArgumentException( "Cannot create weak event to anonymous method with closure." );
            }

            Debug.Assert( getTarget != null );

            var dm = new DynamicMethod(
                "FastSmartWeakEvent", typeof (bool), forwarderParameters, method.DeclaringType );

            ILGenerator il = dm.GetILGenerator();

            if ( !method.IsStatic )
            {
                il.Emit( OpCodes.Ldarg_0 );
                il.EmitCall( OpCodes.Callvirt, getTarget, null );
                il.Emit( OpCodes.Dup );
                Label label = il.DefineLabel();
                il.Emit( OpCodes.Brtrue, label );
                il.Emit( OpCodes.Pop );
                il.Emit( OpCodes.Ldc_I4_1 );
                il.Emit( OpCodes.Ret );
                il.MarkLabel( label );
            }
            il.Emit( OpCodes.Ldarg_1 );
            il.Emit( OpCodes.Ldarg_2 );
            il.EmitCall( OpCodes.Call, method, null );
            il.Emit( OpCodes.Ldc_I4_0 );
            il.Emit( OpCodes.Ret );

            var fd = (ForwarderDelegate) dm.CreateDelegate( typeof (ForwarderDelegate) );
            lock ( forwarders )
            {
                forwarders[method] = fd;
            }
            return fd;
        }

        #region Nested type: ForwarderDelegate

        internal delegate bool ForwarderDelegate( WeakReference wr, object sender, EventArgs e );

        #endregion
    }
}