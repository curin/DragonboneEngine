#pragma once
#include "ByteArray.h"

using namespace System;

namespace Dragonbone
{
    namespace Native
    {
        public ref class BinaryArray
        {
            ::ByteArray* _array;

        public:
            BinaryArray(int size)
            {
                _array = new ::ByteArray(size);
            }

            BinaryArray(::ByteArray* copy)
            {
                _array = new ::ByteArray(copy);
            }

            BinaryArray(BinaryArray^ copy)
            {
                _array = new ::ByteArray(copy->_array);
            }

            ~BinaryArray()
            {
                delete _array;
            }

            property int Length
            {
                int get()
                {
                    return _array->Length();
                }
            }

            void* GetPointer(int index)
            {
                return _array->GetPointer(index);
            }

            IntPtr^ GetManagedPointer(int index)
            {
                return gcnew IntPtr(_array->GetPointer(index));
            }

            Byte GetByteValue(int index)
            {
                return _array->GetByteValue(index);
            }

            void SetByteValue(int index, Byte value)
            {
                _array->SetByteValue(index, value);
            }

            void SetPointer(int index, void* value, int size)
            {
                _array->SetValue(index, value, size);
            }

            void SetManagedPointer(int index, IntPtr^ value, int size)
            {
                _array->SetValue(index, value->ToPointer(), size);
            }

            operator ::ByteArray()
            {
                return _array;
            }

            void CopyTo(array<Byte>^ dest)
            {
                if (dest->Length < Length)
                    throw gcnew ArgumentException("Binary Arrays must be copied to Byte arrays of the same length");
                
                for (int i = 0; i < Length; i++)
                    dest[i] = _array->GetByteValue(i);
            }
        };
    }
}
