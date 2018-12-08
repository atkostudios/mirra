using System;
using System.Reflection;
using Utility;

namespace Ducktype.Models
{
    public class FieldModel : AccessorModel
    {
        internal static FieldModel Create(Type owner, FieldInfo field)
        {
            return new FieldModel(owner, field);
        }

        public override bool IsPublic { get; }

        public override bool CanGet => true;
        public override bool CanSet => true;

        public bool IsBacking { get; }

        public FieldInfo Field => (FieldInfo) Member;

        public FieldModel(Type owner, FieldInfo member) : base(owner, member)
        {
            IsBacking = TypeUtility.IsBackingField(Field);
        }
    }
}