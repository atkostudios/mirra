using System.Reflection;
using NullGuard;

using Atko.Mirra.Generation;

namespace Atko.Mirra
{
    /// <summary>
    /// Wrapper class for <see cref="MethodInfo"/> that provides extended functionality and reflection performance.
    /// </summary>
    public class MethodImage : CallableImage
    {
        /// <summary>
        /// The inner system method of the method image.
        /// </summary>
        public MethodInfo Method => (MethodInfo)Member;

        /// <summary>
        /// The type of object returned by the method.
        /// </summary>
        public TypeImage ReturnType => Method.ReturnType;

        internal MethodImage(MethodInfo method) : base(method,
            (argumentCount) => CodeGenerator.InstanceMethod(method, argumentCount),
            (argumentCount) => CodeGenerator.StaticMethod(method, argumentCount))
        { }

        /// <summary>
        /// Invoke the method with the provided arguments and return the result. Returns null if the return type is
        /// void.
        /// If the property or field is non-static, the instance parameter must be an instance of the correct type.
        /// If the property or field is static, the instance parameter must be null.
        /// </summary>
        /// <param name="arguments">The arguments to provide to the method.</param>
        /// <returns>The value returned from the method or null if the return type is void.</returns>
        [return: AllowNull]
        public object Call([AllowNull] object instance, params object[] arguments)
        {
            return CallInternal(instance, arguments);
        }
    }
}