using System;
using NullGuard;
using Utility;

namespace Ducktype
{
    public struct Duck
    {
        static string DuckTypeErrorMessage { get; } = StringUtility.Paragraph(@"
            The object of type '{0}' cannot be duck typed as '{1}'.
        ");

        static string GetErrorMessage { get; } = StringUtility.Paragraph(@"
            Failed to get the value of the field or property '{0}'. The type '{1}' does not have the specified field
            or property.
        ");

        static string SetErrorMessage { get; } = StringUtility.Paragraph(@"
            Failed to set the value of the field or property '{0}'. The type '{1}' either has does not have the
            specified field or property, or the field or property is a non-auto, get-only property.
        ");

        static string CallErrorMessage { get; } = StringUtility.Paragraph(@"
            Failed to call the method '{0}'. The type '{1}' does not have the specified method.
        ");

        /// <summary>
        /// The underlying object the <see cref="Duck"/> is accessing. This property is null if the <see cref="Duck"/>
        /// wraps a type and not an instance.
        /// </summary>
        [AllowNull]
        public object Instance { get; }

        /// <summary>
        /// The type the <see cref="Duck"/>'s underlying <see cref="Instance"/> object is duck typed as.
        /// </summary>
        public Type Type { get; }

        /// <inheritdoc />
        /// <summary>
        /// Create a <see cref="T:Ducktype.Duck" /> instance wrapper over the provided object instance. The instance
        /// will be duck typed to its true type.
        /// </summary>
        /// <param name="instance">The object to duck type.</param>
        public Duck(object instance) : this(instance, instance.GetType()) { }

        /// <summary>
        /// Create a <see cref="Duck"/> instance wrapper over the provided object instance. The instance will be duck
        /// typed to the specified type.
        /// </summary>
        /// <param name="instance">The object to duck type.</param>
        /// <param name="type">The type to duck type the object to.</param>
        public Duck(object instance, Type type)
        {
            var implementation = TypeProcessor.GetImplementation(instance.GetType(), type);
            if (implementation == null)
            {
                throw new DucktypeException(string.Format(DuckTypeErrorMessage, instance.GetType(), type));
            }

            Instance = instance;
            Type = implementation;
        }

        /// <summary>
        /// Create a <see cref="Duck"/> static wrapper over the provided type's static members.
        /// </summary>
        /// <param name="type">The type to duck type.</param>
        public Duck(Type type)
        {
            Instance = null;
            Type = type;
        }

        /// <summary>
        /// Dynamically get the value of a field or property with a specified name.
        /// </summary>
        /// <param name="name">The name of the field or property to get.</param>
        /// <returns>The value of the field or property.</returns>
        /// <exception cref="DucktypeException">Thrown when the field or property cannot be accessed.</exception>
        [return: AllowNull]
        public object Get(string name)
        {
            if (TypeProcessor.Get(Instance, Type, name, out var value))
            {
                return value;
            }

            throw new DucktypeException(string.Format(GetErrorMessage, name, Type));
        }

        /// <summary>
        /// Dynamically set the value of a field or property with a specified name to an specified value.
        /// </summary>
        /// <param name="name">The name of the field or property to set.</param>
        /// <param name="value">The value the field or property should be set to.</param>
        /// <exception cref="DucktypeException">Thrown when the field or property cannot be set.</exception>
        public void Set(string name, [AllowNull] object value)
        {
            if (TypeProcessor.Set(Instance, Type, name, value))
            {
                return;
            }

            throw new DucktypeException(string.Format(SetErrorMessage, name, Type));
        }

        /// <summary>
        /// Dynamically call a method with a specified name using the provided arguments.
        /// </summary>
        /// <param name="name">The name of the method to call.</param>
        /// <param name="arguments">The arguments to pass into the method.</param>
        /// <returns>The result of the function call, or null if the method's return type is void.</returns>
        /// <exception cref="DucktypeException">Thrown when the method cannot be invoked.</exception>
        [return: AllowNull]
        public object Call(string name, params object[] arguments)
        {
            if (TypeProcessor.Call(Instance, Type, name, out var result, arguments))
            {
                return result;
            }

            throw new DucktypeException(string.Format(CallErrorMessage, name, Type));
        }

        /// <summary>
        /// Get or set the value of an indexer on an object such as an array or dictionary.
        /// </summary>
        /// <param name="index">The parameters to pass to the indexer.</param>
        /// <exception cref="DucktypeException">Thrown when the indexer cannot be accessed or set.</exception>
        [AllowNull]
        public object this[params object[] index]
        {
            get
            {
                if (TypeProcessor.GetElement(Instance, Type, index, out var value))
                {
                    return value;
                }

                throw new DucktypeException();
            }
            set
            {
                if (TypeProcessor.SetElement(Instance, Type, index, value))
                {
                    return;
                }

                throw new DucktypeException();
            }
        }

        public Type GetImplementation(Type type)
        {
            return TypeProcessor.GetImplementation(Type, type);
        }
    }
}