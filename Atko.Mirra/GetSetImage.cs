using System;
using System.Reflection;

namespace Atko.Mirra
{
    /// <summary>
    /// Wrapper class for <see cref="PropertyInfo"/>, <see cref="FieldInfo"/> and <see cref="PropertyInfo"/> indexers
    /// that provides extended functionality and reflection performance.
    /// </summary>
    public abstract class GetSetImage : TypeMemberImage
    {
        /// <summary>
        /// True if the property, field or indexer can be set.
        /// </summary>
        public abstract bool CanSet { get; }

        /// <summary>
        /// The type the property, field or indexer contains or maps to.
        /// </summary>
        public abstract TypeImage DeclaredType { get; }

        internal GetSetImage(MemberInfo member) : base(member) { }

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