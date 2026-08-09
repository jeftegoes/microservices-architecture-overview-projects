using System;
using System.Collections.Generic;

namespace Messaging.Interfaces.SharedLib.Events
{
    public interface IOrderProcessedEvent
    {
        Guid OrderId { get; }
        string PirctureUrl { get; }
        List<byte[]> Faces { get; }
        String UserEmail { get; }
    }
}