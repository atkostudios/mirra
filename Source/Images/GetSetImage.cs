using System;
using System.Reflection;

namespace Atko.Mirra.Images
{
    public abstract class GetSetImage : MemberImage
    {
        public bool CanGet => true;

        public abstract bool CanSet { get; }
        public abstract Type Type { get; }

        protected GetSetImage(MemberInfo member) : base(member) { }

        protected void AssertCanSet()
        {
            if (CanSet)
            {
                return;
            }

            throw new MirraInvocationCannotSetException();
        }
    }
}