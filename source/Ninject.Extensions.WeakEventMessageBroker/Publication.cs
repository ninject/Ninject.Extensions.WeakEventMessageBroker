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
using System.Reflection;

#endregion

namespace Ninject.Extensions.WeakEventMessageBroker
{
    /// <summary>
    /// Tracks the publisher of an event so that it can be unhooked when the channel is closed.
    /// </summary>
    public class Publication
    {
        /// <summary>
        /// 
        /// </summary>
        public Delegate Method { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public WeakReference Instance { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public EventInfo Event { get; set; }
    }
}