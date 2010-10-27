#region License

// 
// Author: Ian Davis <ian@innovatian.com>
// Copyright (c) 2009-2010, Innovatian Software, LLC
//
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
// See the file LICENSE.txt for details.
// 

#endregion

#if !NO_LCG && !SILVERLIGHT

#region Using Directives

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace Ninject.Extensions.WeakEventMessageBroker
{
    /// <summary>
    /// Generate IL for DynamicMethods
    /// </summary>
    internal static class ILEmitter
    {
        internal static void EmitGuardCondition( ILGenerator il, MethodBase method, MethodInfo predicate )
        {
            // predicate returns 0 or null, bail
            il.Emit( OpCodes.Ldarg_0 );
            EmitMethodCall( il, predicate );
            il.Emit( OpCodes.Dup );
            EmitReturnIfNull( il );
        }

        internal static void EmitMethodCall( ILGenerator il, MethodInfo method )
        {
            OpCode opCode = method.IsFinal ? OpCodes.Call : OpCodes.Callvirt;
            il.Emit( opCode, method );
        }

        internal static void EmitReturnIfNull( ILGenerator il )
        {
            Label label = il.DefineLabel();
            il.Emit( OpCodes.Brtrue, label );
            il.Emit( OpCodes.Pop );
            EmitReturn( il );
            il.MarkLabel( label );
        }

        internal static void EmitReturn( ILGenerator il )
        {
            il.Emit( OpCodes.Ret );
        }

        internal static void EmitEventHandlerMethodCall( ILGenerator il, MethodInfo method,
                                                         IEnumerable<OpCode> arguments )
        {
            foreach ( OpCode opCode in arguments )
            {
                il.Emit( opCode );
            }
            EmitMethodCall( il, method );
        }
    }
}
#endif //!NO_LCG