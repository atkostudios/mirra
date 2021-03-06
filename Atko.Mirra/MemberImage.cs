using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NullGuard;

using Atko.Mirra.Utilities;

namespace Atko.Mirra
{
    /// <summary>
    /// Wrapper class for <see cref="MemberInfo"/> that provides extended functionality and reflection performance.
    /// </summary>
    public abstract class MemberImage
    {
        /// <summary>
        /// The name of the member.
        /// </summary>
        public string Name => Member.Name;

        /// <summary>
        /// The non-qualified name of the member.
        /// </summary>
        public string ShortName { get; }

        /// <summary>
        /// True if the member was generated by the compiler.
        /// </summary>
        public bool IsCompilerGenerated { get; }

        /// <summary>
        /// The type in which the member was declared.
        /// </summary>
        [AllowNull]
        public TypeImage DeclaringType => Member.DeclaringType;

        /// <summary>
        /// The inner system member.
        /// </summary>
        public MemberInfo Member { get; }

        /// <summary>
        /// True if the member has public accessability.
        /// </summary>
        public abstract bool IsPublic { get; }

        /// <summary>
        /// True if the member is declared as static.
        /// </summary>
        public abstract bool IsStatic { get; }

        internal MemberImage(MemberInfo member)
        {
            Member = member;
            ShortName = Member.Name.SubstringAfterLast(".");
            IsCompilerGenerated = HasAttribute<CompilerGeneratedAttribute>();
        }

        /// <summary>
        /// Return true if the member has an attribute inheriting or implementing the provided type.
        /// </summary>
        /// <param name="type">The type of attribute to query.</param>
        /// <param name="inherit">Set to true to query inherited attributes.</param>
        /// <returns>A boolean value representing if the member has an attribute of the given type.</returns>
        public bool HasAttribute(Type type, bool inherit = true)
        {
            return Member.IsDefined(type, inherit);
        }

        /// <summary>
        /// Return true if the member has an attribute inheriting or implementing the provided type.
        /// </summary>
        /// <typeparam name="T">The type of attribute to query.</typeparam>
        /// <param name="inherit">Set to true to query inherited attributes.</param>
        /// <returns>A boolean value representing if the member has an attribute of the given type.</returns>
        public bool HasAttribute<T>(bool inherit = true)
        {
            return HasAttribute(typeof(T), inherit);
        }

        /// <summary>
        /// Return the attribute on the member inheriting or implementing the provided type. Returns null if the
        /// attribute does not exist.
        /// </summary>
        /// <param name="type">The type of attribute to query.</param>
        /// <param name="inherit">Set to true to query inherited attributes.</param>
        /// <returns>The attribute of the provided type or null.</returns>
        [return: AllowNull]
        public Attribute Attribute(Type type, bool inherit = true)
        {
            return Member.GetCustomAttribute(type, inherit);
        }

        /// <summary>
        /// Return the attribute on the member inheriting or implementing the provided type. Returns null if the
        /// attribute does not exist.
        /// </summary>
        /// <typeparam name="T">The type of attribute to query.</param>
        /// <param name="inherit">Set to true to query inherited attributes.</param>
        /// <returns>The attribute of the provided type or null.</returns>
        [return: AllowNull]
        public T Attribute<T>(bool inherit = true) where T : class
        {
            return Attribute(typeof(T), inherit) as T;
        }

        /// <summary>
        /// Return all attributes on the member inheriting or implementing the provided type.
        /// </summary>
        /// <param name="type">The type of attribute to query.</param>
        /// <param name="inherit">Set to true to query inherited attributes.</param>
        /// <returns>All attributes of the provided type.</returns>
        public IEnumerable<Attribute> Attributes(Type type, bool inherit = true)
        {
            return Member.GetCustomAttributes(type, inherit).Cast<Attribute>();
        }

        /// <summary>
        /// Return all attributes on the member.
        /// </summary>
        /// <param name="inherit">Set to true to query inherited attributes.</param>
        /// <returns>All attributes.</returns>
        public IEnumerable<Attribute> Attributes(bool inherit = true)
        {
            return Member.GetCustomAttributes(inherit).Cast<Attribute>();
        }

        /// <summary>
        /// Return all attributes on the member inheriting or implementing the provided type.
        /// </summary>
        /// <typeparam name="T">The type of attribute to query.</param>
        /// <param name="inherit">Set to true to query inherited attributes.</param>
        /// <returns>All attributes of the provided type.</returns>
        public IEnumerable<T> Attributes<T>(bool inherit = true) where T : class
        {
            return Attributes(typeof(T), inherit).Cast<T>();
        }

        protected static void CheckException(Exception exception)
        {
            var isTypeException = exception is InvalidCastException || exception is NullReferenceException;
            if (isTypeException && new StackTrace(exception).FrameCount <= 2)
            {
                throw new MirraInvocationArgumentException();
            }

            throw new MirraInvocationException(null, exception);
        }
    }
}