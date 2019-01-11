using System;
using System.Reflection;
using Atko.Mirra.Utility;

namespace Atko.Mirra.Images
{
    /// <summary>
    /// Wrapper class for <see cref="FieldInfo"/> that provides extended functionality and reflection performance.
    /// </summary>
    public class FieldImage : AccessorImage
    {
        /// <inheritdoc/>
        public override bool IsPublic => Field.IsPublic;

        /// <inheritdoc/>
        public override bool IsStatic => Field.IsStatic;

        /// <inheritdoc/>
        public override bool CanSet { get; }

        /// <inheritdoc/>
        public override TypeImage DeclaredType => TypeImage.Get(Field.FieldType);

        /// <summary>
        /// True if the field is a backing field for a property.
        /// </summary>
        public bool IsBacking { get; }

        /// <summary>
        /// True if the field is declared readonly.
        /// </summary>
        public bool IsReadOnly => Field.IsInitOnly;

        /// <summary>
        /// The inner system field.
        /// </summary>
        public FieldInfo Field => (FieldInfo)Member;

        internal FieldImage(FieldInfo member) : base(member)
        {
            IsBacking = TypeUtility.IsBackingField(Field);
            CanSet = GetCanSet();
        }

        bool GetCanSet()
        {
            if (IsReadOnly && IsStatic && TypeUtility.CanBeConstantStruct(DeclaredType))
            {
                return false;
            }

            return true;
        }
    }
}