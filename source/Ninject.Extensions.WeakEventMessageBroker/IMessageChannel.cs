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