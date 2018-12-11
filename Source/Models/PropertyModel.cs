using System;
using System.Reflection;
using Atko.Dodge.Utility;
using NullGuard;

namespace Atko.Dodge.Models
{
    public class PropertyModel : AccessorModel
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
        public FieldModel BackingField => LazyBackingField.Value;

        public PropertyInfo Property => (PropertyInfo) Member;

        Lazy<FieldModel> LazyBackingField { get; }

        internal PropertyModel(Type owner, PropertyInfo member) : base(owner, member)
        {
            if (!CanCreateFrom(member))
            {
                throw new ArgumentException(nameof(member));
            }

            LazyBackingField = new Lazy<FieldModel>(GetBackingField);
        }

        FieldModel GetBackingField()
        {
            return TypeModel.Get(Property.DeclaringType).GetField(TypeUtility.GetBackingFieldName(Property));
        }
    }
}