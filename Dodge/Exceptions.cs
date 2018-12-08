using System;

namespace Atko.Dodge
{
    public class DodgeException : Exception
    {
        public DodgeException(string message = null, Exception inner = null) : base(message, inner) { }
    }

    public class DodgeInvocationException : DodgeException
    {
        public DodgeInvocationException(string message = null, Exception inner = null) :
            base(message ?? "An invocation failed.", inner) { }
    }

    public class DodgeMissingMemberException : DodgeException
    {
        public DodgeMissingMemberException(string message = null, Exception inner = null) :
            base(message ?? "Member does not exist on the target type.", inner) { }
    }
}