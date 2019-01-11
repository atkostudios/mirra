using System;
using System.Diagnostics;
using System.Reflection;
using Atko.Mirra.Utility;
using NullGuard;

namespace Atko.Mirra.Images
{
    /// <summary>
    /// Wrapper class for <see cref="PropertyInfo"/> that provides extended functionality and reflection performance.
    /// </summary>
    public class PropertyImage : AccessorImage
    {
        internal static bool CanCreateFrom(PropertyInfo property)
        {
            return property.GetIndexParameters().Length == 0;
        }

        /// <inheritdoc/>
        public override bool IsPublic => Property.GetMethod.IsPublic || (Property.SetMethod?.IsPublic ?? false);

        /// <inheritdoc/>
        public override bool IsStatic => Property.GetMethod.IsStatic;

        /// <inheritdoc/>
        public override bool CanSet { get; }

        /// <inheritdoc/>
        public override TypeImage DeclaredType => TypeImage.Get(Property.PropertyType);

        /// <summary>
        /// The backing field of the property. Null if the property is not an auto-property.
        /// </summary>
        [AllowNull]
        public FieldImage BackingField { get; }

        /// <summary>
        /// The inner system property.
        /// </summary>
        public PropertyInfo Property => (PropertyInfo)Member;

        internal PropertyImage(PropertyInfo member) : base(member)
        {
            Debug.Assert(CanCreateFrom(member));

            BackingField = GetBackingField();
            CanSet = GetCanSet();
        }

        bool GetCanSet()
        {
            if (Property.CanWrite)
            {
                return true;
            }

            if (BackingField != null && BackingField.CanSet)
            {
                return true;
            }

            return false;
        }

        FieldImage GetBackingField()
        {
            return TypeImage.Get(DeclaringType).Field(TypeUtility.GetBackingFieldName(Property));
        }
    }
}