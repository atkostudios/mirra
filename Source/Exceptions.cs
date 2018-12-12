using System;

namespace Atko.Mirra
{
    public class MirraException : Exception
    {
        public MirraException(string message = null, Exception inner = null) : base(message, inner) { }
    }

    public class MirraInvocationException : MirraException
    {
        public MirraInvocationException(string message = null, Exception inner = null) :
            base(message ?? "An invocation failed.", inner) { }
    }

    public class MirraMissingMemberException : MirraException
    {
        public MirraMissingMemberException(string message = null, Exception inner = null) :
            base(message ?? "Member does not exist on the target type.", inner) { }
    }
}