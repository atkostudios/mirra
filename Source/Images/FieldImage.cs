using System;
using System.Reflection;
using Atko.Mirra.Utility;

namespace Atko.Mirra.Images
{
    public class FieldImage : AccessorImage
    {
        public override bool IsPublic => Field.IsPublic;
        public override bool IsStatic => Field.IsStatic;

        public override bool CanSet { get; }

        public override Type Type => Field.FieldType;

        public bool IsBacking { get; }
        public bool IsReadOnly => Field.IsInitOnly;

        public FieldInfo Field => (FieldInfo)Member;

        internal FieldImage(FieldInfo member) : base(member)
        {
            IsBacking = TypeUtility.IsBackingField(Field);
            CanSet = GetCanSet();
        }

        bool GetCanSet()
        {
            if (IsReadOnly && IsStatic && TypeUtility.CanBeConstantStruct(Type))
            {
                return false;
            }

            return true;
        }
    }
}