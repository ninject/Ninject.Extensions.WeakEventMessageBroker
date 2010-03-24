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

#endregion

namespace Ninject.Extensions.WeakEventMessageBroker
{
    internal struct TransportCacheEntry
    {
        public readonly WeakReference Target;
        public readonly Action<WeakReference, object, EventArgs> Transport;

        public TransportCacheEntry( Action<WeakReference, object, EventArgs> transport,
                                    WeakReference target )
        {
            Transport = transport;
            Target = target;
        }
    }
}