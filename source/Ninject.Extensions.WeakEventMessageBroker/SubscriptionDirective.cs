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
    public class SubscriptionDirective : IDirective
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionDirective"/> class.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="methodInfo">The method info.</param>
        public SubscriptionDirective( string channel, MethodInfo methodInfo )
        {
            Channel = channel;
            MethodInfo = methodInfo;
        }

        /// <summary>
        /// Gets the channel.
        /// </summary>
        /// <value>The channel.</value>
        public string Channel { get; private set; }

        /// <summary>
        /// Gets the method info used to call back on the subscriber when a
        /// message is received on the channel
        /// </summary>
        /// <value>The method info.</value>
        public MethodInfo MethodInfo { get; private set; }
    }
}