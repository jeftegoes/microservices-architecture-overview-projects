using System;

namespace Messaging.Interfaces.SharedLib.Events
{
    public interface IOrderDispatchedEvent
    {
        Guid OrderId { get; }
        DateTime DispatchDateTime { get; }
    }
}