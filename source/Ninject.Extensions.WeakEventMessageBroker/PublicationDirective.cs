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

using System.Reflection;
using Ninject.Planning.Directives;

#endregion

namespace Ninject.Extensions.WeakEventMessageBroker
{
    /// <summary>
    /// 
    /// </summary>
    public class PublicationDirective : IDirective
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublicationDirective"/> class.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="eventInfo">The event tied to the channel.</param>
        public PublicationDirective( string channel, EventInfo eventInfo )
        {
            Channel = channel;
            Event = eventInfo;
        }

        /// <summary>
        /// Gets the channel.
        /// </summary>
        /// <value>The channel.</value>
        public string Channel { get; private set; }

        /// <summary>
        /// Gets the event tied to the channel.
        /// </summary>
        /// <value>The event.</value>
        public EventInfo Event { get; private set; }
    }
}