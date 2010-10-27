#region License

// 
// Author: Nate Kohari <nate@enkari.com>
// Copyright (c) 2007-2010, Enkari, Ltd.
// 
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
// See the file LICENSE.txt for details.
// 

#endregion

#region Using Directives

using System.Threading;

#endregion

namespace Ninject.Extensions.WeakEventMessageBroker
{
    /// <summary>
    /// Selects the thread context that a message should be delivered on.
    /// </summary>
    public enum DeliveryThread
    {
        /// <summary>
        /// Indicates that the message should be delivered on the current thread.
        /// </summary>
        Current,

        /// <summary>
        /// Indicates that the message should be delivered asynchronously via the <see cref="ThreadPool"/>.
        /// </summary>
        Background,

#if !NETCF
        /// <summary>
        /// Indicates that the message should be delivered on the thread that owns the user interface,
        /// or the current thread if no synchronization context exists.
        /// </summary>
        UserInterface,
#endif
    }
}