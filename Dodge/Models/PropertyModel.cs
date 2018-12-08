using System;
using System.Reflection;
using Atko.Dodge.Utility;
using NullGuard;

namespace Atko.Dodge.Models
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

        public override bool IsPublic => (Property.GetMethod?.IsPublic ?? false) ||
                                         (Property.SetMethod?.IsPublic ?? false);

        public override bool CanGet => Property.CanRead || BackingField != null;
        public override bool CanSet => Property.CanWrite;

        [AllowNull]
        public FieldModel BackingField => LazyBackingField.Value;

        public PropertyInfo Property => (PropertyInfo) Member;

        Lazy<FieldModel> LazyBackingField { get; }

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