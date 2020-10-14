using System;

namespace SolidCqrsFramework.Exceptions
{
    public class CommandHandlerNotFoundException : Exception
    {
        public CommandHandlerNotFoundException() { }
        public CommandHandlerNotFoundException(string message) : base(message) { }
    }
}
