#region Using Directives

using System;

#endregion

namespace Ninject.Extensions.WeakEventMessageBroker.Tests
{
    public class MessageEventArgs : EventArgs
    {
        public MessageEventArgs( string message )
        {
            Message = message;
        }

        public string Message { get; private set; }
    }
}