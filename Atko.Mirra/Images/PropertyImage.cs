using System;
using System.Diagnostics;
using System.Reflection;
using Atko.Mirra.Utility;
using NullGuard;

namespace Atko.Mirra.Images
{
    public class PropertyImage : AccessorImage
    {
        public static bool CanCreateFrom(PropertyInfo property)
        {
            return property.GetIndexParameters().Length == 0;
        }

        public override bool IsPublic => Property.GetMethod.IsPublic || (Property.SetMethod?.IsPublic ?? false);
        public override bool IsStatic => Property.GetMethod.IsStatic;

        public override bool CanSet { get; }

        public override Type Type => Property.PropertyType;

        [AllowNull]
        public FieldImage BackingField { get; }

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