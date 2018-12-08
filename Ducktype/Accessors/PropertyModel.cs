using System;
using System.Reflection;
using NullGuard;
using Utility;

namespace Ducktype.Models
{
    public class PropertyModel : AccessorModel
    {
        [return: AllowNull]
        internal static PropertyModel Create(Type owner, PropertyInfo property)
        {
            if (property.GetIndexParameters().Length > 0)
            {
                return null;
            }

            return new PropertyModel(owner, property);
        }

        public override bool IsPublic => Property.GetMethod.IsPublic || Property.SetMethod.IsPublic;

        public override bool CanGet => Property.CanRead || BackingField != null;
        public override bool CanSet => Property.CanWrite;

        [AllowNull]
        public FieldModel BackingField => LazyBackingField.Value;

        Lazy<FieldModel> LazyBackingField { get; }

        PropertyInfo Property { get; }

        PropertyModel(Type owner, PropertyInfo property) : base(owner, property)
        {
            LazyBackingField = new Lazy<FieldModel>(GetBackingField);
        }

        FieldModel GetBackingField()
        {
            return TypeModel.Get(Property.DeclaringType).GetField(TypeUtility.GetBackingFieldName(Property));
        }
    }
}