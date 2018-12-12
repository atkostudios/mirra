using System;
using System.Reflection;
using Atko.Dodge.Utility;
using NullGuard;

namespace Atko.Dodge.Images
{
    public class PropertyImage : AccessorImage
    {
        public static bool CanCreateFrom(PropertyInfo property)
        {
            return property.GetIndexParameters().Length == 0;
        }

        public override bool IsPublic => Property.GetMethod.IsPublic || (Property.SetMethod?.IsPublic ?? false);
        public override bool IsStatic => Property.GetMethod.IsStatic;

        public override bool CanGet => Property.CanRead || BackingField != null;
        public override bool CanSet => Property.CanWrite || BackingField != null;

        [AllowNull]
        public FieldImage BackingField => LazyBackingField.Value;

        public PropertyInfo Property => (PropertyInfo)Member;

        Lazy<FieldImage> LazyBackingField { get; }

        internal PropertyImage(Type owner, PropertyInfo member) : base(owner, member)
        {
            if (!CanCreateFrom(member))
            {
                throw new ArgumentException(nameof(member));
            }

            LazyBackingField = new Lazy<FieldImage>(GetBackingField);
        }

        FieldImage GetBackingField()
        {
            return TypeImage.Get(Property.DeclaringType).Field(TypeUtility.GetBackingFieldName(Property));
        }
    }
}