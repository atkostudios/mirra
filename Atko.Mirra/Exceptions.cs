using System;

namespace Atko.Mirra
{
    public class MirraException : Exception
    {
        public MirraException(string message = null, Exception inner = null) : base(message, inner)
        { }
    }

    public class MirraInvocationException : MirraException
    {
        public MirraInvocationException(string message = null, Exception inner = null) :
            base(message ?? "An invocation failed.", inner)
        { }
    }

    public class MirraMissingMemberException : MirraException
    {
        public MirraMissingMemberException(string message = null, Exception inner = null) :
            base(message ?? "Member does not exist on the target type.", inner)
        { }
    }

    public class MirraInvocationCannotSetException : MirraException
    {
        public MirraInvocationCannotSetException() :
            base("Cannot set accessor or indexer.")
        { }
    }

    public class MirraInvocationArgumentCountException : MirraInvocationException
    {
        public MirraInvocationArgumentCountException() :
            base("Invalid number of arguments for invocation.")
        { }
    }

    public class MirraInvocationArgumentException : MirraInvocationException
    {
        public MirraInvocationArgumentException() :
            base("Invalid instance or argument types for invocation.")
        { }
    }
}