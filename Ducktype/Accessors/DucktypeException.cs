using System;

namespace Ducktype.Models
{
    public class DucktypeException : Exception
    {
        public DucktypeException(string message = null, Exception inner = null) : base(message, inner) { }
    }

    public class DucktypeInvocationException : DucktypeException
    {
        public DucktypeInvocationException(string message = null, Exception inner = null) :
            base(message ?? "An invocation failed.", inner) { }
    }

    public class DucktypeMissingMemberException : DucktypeException
    {
        public DucktypeMissingMemberException(string message = null, Exception inner = null) :
            base(message ?? "Member does not exist on the target type.", inner) { }
    }
}