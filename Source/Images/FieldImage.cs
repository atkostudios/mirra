using System;
using System.Reflection;
using Atko.Mirra.Utility;

namespace Atko.Mirra.Images
{
    public class FieldImage : AccessorImage
    {
        internal static FieldImage Create(Type owner, FieldInfo field)
        {
            return new FieldImage(owner, field);
        }

        public override bool IsPublic => Field.IsPublic;
        public override bool IsStatic => Field.IsStatic;

        public override bool CanGet => true;
        public override bool CanSet => true;

        public bool IsBacking { get; }

        public FieldInfo Field => (FieldInfo)Member;

        FieldImage(Type owner, FieldInfo member) : base(owner, member)
        {
            IsBacking = TypeUtility.IsBackingField(Field);
        }
    }
}