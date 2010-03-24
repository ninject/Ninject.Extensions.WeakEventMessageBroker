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

using Ninject.Activation.Strategies;
using Ninject.Modules;
using Ninject.Planning.Strategies;

#endregion

namespace Ninject.Extensions.WeakEventMessageBroker
{
    /// <summary>
    /// Configures the kernel integrating the message broker.
    /// </summary>
    public class WeakEventMessageBrokerModule : NinjectModule
    {
        /// <summary>
        /// Loads the module into the kernel.
        /// </summary>
        public override void Load()
        {
            Kernel.Components.Add<IPlanningStrategy, EventReflectionStrategy>();
            Kernel.Components.Add<IActivationStrategy, EventBindingStrategy>();
            Kernel.Components.Add<IWeakEventMessageBroker, WeakEventMessageBroker>();
        }

        /// <summary>
        /// Unloads the module from the kernel.
        /// </summary>
        public override void Unload()
        {
            Kernel.Components.RemoveAll<EventReflectionStrategy>();
            Kernel.Components.RemoveAll<EventBindingStrategy>();
            Kernel.Components.RemoveAll<IWeakEventMessageBroker>();
        }
    }
}