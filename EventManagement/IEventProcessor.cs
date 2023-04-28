using System;
using System.Threading.Tasks;

namespace SolidCqrsFramework.EventManagement;

public interface IEventProcessor
{
    Task ProcessEventAsync(Type eventType, dynamic message);
}
