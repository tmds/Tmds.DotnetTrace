using System;

namespace Tmds.Babeltrace
{
    public unsafe struct FieldDeclaration
    {
        private readonly void* _fieldDeclaration;

        internal FieldDeclaration(void* fieldDeclaration)
        {
            _fieldDeclaration = fieldDeclaration;
        }

        public FieldType Type
            => Interop.GetFieldType(_fieldDeclaration);

        public int ArrayLength
            => Interop.GetArrayLength(_fieldDeclaration);

        public bool IsIntegerSigned
            => Interop.GetArrayLength(_fieldDeclaration) == 1;

        public int IntegerBase
            => Interop.GetIntegerBase(_fieldDeclaration);

        public int IntegerBitLength
            => Interop.GetIntegerBitLength(_fieldDeclaration).ToInt32();

        public StringEncoding Encoding
            => Interop.GetEncoding(_fieldDeclaration);
    }
}