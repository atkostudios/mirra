using System;

namespace Ducktype
{
    public class DucktypeException : Exception
    {
        public DucktypeException(string message = null, Exception inner = null) : base(message, inner) { }
    }
}