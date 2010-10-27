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

using System;

#endregion

namespace Ninject.Extensions.WeakEventMessageBroker
{
    /// <summary>
    /// Indicates that the decorated event should be published into a message broker channel.
    /// </summary>
    [AttributeUsage( AttributeTargets.Event, AllowMultiple = true, Inherited = true )]
    public sealed class PublishAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishAttribute"/> class.
        /// </summary>
        /// <param name="channel">The name of the channel to publish to.</param>
        public PublishAttribute( string channel )
        {
            Channel = channel;
        }

        /// <summary>
        /// Gets the name of the channel to publish to.
        /// </summary>
        public string Channel { get; private set; }
    }
}