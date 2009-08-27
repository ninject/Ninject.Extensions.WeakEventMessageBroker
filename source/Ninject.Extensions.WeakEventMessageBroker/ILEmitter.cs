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

        internal static void EmitEventHandlerMethodCall( ILGenerator il, MethodInfo method, IEnumerable<OpCode> arguments )
        {
            foreach ( OpCode opCode in arguments )
            {
                il.Emit( opCode );
            }
            EmitMethodCall( il, method );
        }
    }
}