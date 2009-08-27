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
using System.Reflection;

#endregion

namespace Ninject.Extensions.WeakEventMessageBroker
{
    /// <summary>
    /// Describes a communication channel used to transport messages.
    /// </summary>
    public interface IMessageChannel : IDisposable
    {
        /// <summary>
        /// Gets the name of the channel.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        ICollection Subscriptions { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="eventInfo"></param>
        void AddPublication( object instance, EventInfo eventInfo );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        void AddSubscription( object instance, MethodInfo method );

        /// <summary>
        /// Closes the channel releasing its resources.
        /// </summary>
        void Close();

        /// <summary>
        /// Enables the channel.
        /// </summary>
        void Enable();

        /// <summary>
        /// Disables the channel.
        /// </summary>
        void Disable();
    }
}