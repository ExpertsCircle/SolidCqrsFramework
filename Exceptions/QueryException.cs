using System;

namespace SolidCqrsFramework.Exceptions;

public class QueryException : Exception
{
    public QueryException(string message) : base(message)
    {
    }
    public QueryException(string message, Exception inner) : base(message, inner)
    {
    }
}
