using System;
using System.Diagnostics;
using System.Reflection;
using Atko.Mirra.Generation;
using Atko.Mirra.Utility;
using NullGuard;

namespace Atko.Mirra.Images
{
    /// <summary>
    /// Wrapper class for <see cref="PropertyInfo"/> indexers which provides extended functionality and reflection
    /// performance.
    /// </summary>
    public class IndexerImage : GetSetImage
    {
        internal static bool CanCreateFrom(PropertyInfo property)
        {
            var parameters = property.GetIndexParameters();
            if (parameters.Length == 0)
            {
                return false;
            }

            if (TypeUtility.HasSpecialParameters(parameters))
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool IsPublic => Property.GetMethod.IsPublic || (Property.SetMethod?.IsPublic ?? false);

        /// <inheritdoc/>
        public override bool IsStatic => Property.GetMethod.IsStatic;

        /// <inheritdoc/>
        public override bool CanSet => Property.SetMethod != null;

        /// <inheritdoc/>
        public override TypeImage DeclaredType => TypeImage.Get(Property.PropertyType);

        /// <summary>
        /// The inner system property.
        /// </summary>
        public PropertyInfo Property => (PropertyInfo)Member;

        InvokerDispatch<InstanceIndexerGetInvoker> GetInvokers { get; }
        InvokerDispatch<InstanceIndexerSetInvoker> SetInvokers { get; }

        internal IndexerImage(PropertyInfo member) : base(member)
        {
            Debug.Assert(CanCreateFrom(member));

            var parameters = Property.GetIndexParameters();
            GetInvokers = new InvokerDispatch<InstanceIndexerGetInvoker>(parameters,
                (argumentCount) => CodeGenerator.InstanceIndexGetter(Property, argumentCount));

            SetInvokers = new InvokerDispatch<InstanceIndexerSetInvoker>(parameters,
                (argumentCount) => CodeGenerator.InstanceIndexSetter(Property, argumentCount));
        }

        /// <summary>
        /// Get the value of the indexer on a provided instance object at a given index.
        /// The instance parameter must be an instance of the correct type.
        /// </summary>
        /// <param name="instance">The instance to index.</param>
        /// <param name="index">The index to invoke the indexer with.</param>
        /// <returns>The value associated with the index.</returns>
        [return: AllowNull]
        public object Get([AllowNull] object instance, object index)
        {
            return Get(instance, new[] { index });
        }

        /// <summary>
        /// Get the value of the indexer on a provided instance object at a given multi-argument index.
        /// The instance parameter must be an instance of the correct type.
        /// </summary>
        /// <param name="instance">The instance to index.</param>
        /// <param name="index">The multi-argument index to invoke the indexer with.</param>
        /// <returns>The value associated with the multi-argument index.</returns>
        [return: AllowNull]
        public object Get([AllowNull] object instance, object[] index)
        {
            AssertInstanceMatches(instance);

            var invoker = GetInvokers.Get(index.Length);
            if (invoker == null)
            {
                throw new MirraInvocationArgumentCountException();
            }

            try
            {
                return invoker.Invoke(instance, index);
            }
            catch (MirraException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CheckException(exception);
                throw;
            }
        }

        /// <summary>
        /// Set the value of the indexer on a provided instance object at a given index.
        /// The instance parameter must be an instance of the correct type.
        /// </summary>
        /// <param name="instance">The instance to index.</param>
        /// <param name="index">The index to invoke the indexer with.</param>
        /// <param name="value">The value to assign to the index.</param>
        public void Set([AllowNull] object instance, object index, object value)
        {
            Set(instance, new[] { index }, value);
        }

        /// <summary>
        /// Set the value of the indexer on a provided instance object at a given index.
        /// The instance parameter must be an instance of the correct type.
        /// </summary>
        /// <param name="instance">The instance to index.</param>
        /// <param name="index">The index to invoke the indexer with.</param>
        /// <param name="value">The value to assign to the index.</param>
        public void Set([AllowNull] object instance, object[] index, object value)
        {
            AssertCanSet();
            AssertInstanceMatches(instance);

            var invoker = SetInvokers.Get(index.Length);
            if (invoker == null)
            {
                throw new MirraInvocationArgumentCountException();
            }

            try
            {
                invoker.Invoke(instance, index, value);
            }
            catch (MirraException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CheckException(exception);
                throw;
            }
        }
    }
}