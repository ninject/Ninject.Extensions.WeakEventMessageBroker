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

using Ninject.Components;

#endregion

namespace Ninject.Extensions.WeakEventMessageBroker
{
    /// <summary>
    /// An object that passes messages between instances in the form of loose-coupled events.
    /// </summary>
    public interface IWeakEventMessageBroker : INinjectComponent
    {
        /// <summary>
        /// Returns a channel with the specified name, creating it first if necessary.
        /// </summary>
        /// <param name="name">The name of the channel to create or retrieve.</param>
        /// <returns>The object representing the channel.</returns>
        IMessageChannel GetChannel( string name );

        /// <summary>
        /// Closes a channel, removing it from the message broker.
        /// </summary>
        /// <param name="name">The name of the channel to close.</param>
        void CloseChannel( string name );

        /// <summary>
        /// Enables a channel, causing it to pass messages through as they occur.
        /// </summary>
        /// <param name="name">The name of the channel to enable.</param>
        void EnableChannel( string name );

        /// <summary>
        /// Disables a channel, which will block messages from being passed.
        /// </summary>
        /// <param name="name">The name of the channel to disable.</param>
        void DisableChannel( string name );
    }
}