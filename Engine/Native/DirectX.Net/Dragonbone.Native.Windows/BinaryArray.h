#pragma once
#include "ByteArray.h"

using namespace System;
using namespace System::Runtime::InteropServices;

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

            void* GetPointerTo(int index)
            {
                return _array->GetPointer(index);
            }

            void* GetPointer(int index)
            {
                return _array->GetValue<void*>(index);
            }

            generic<typename TValue>
            TValue GetValue(int index)
            {
                GCHandle ghandle = GCHandle::FromIntPtr(GetManagedPointerTo(index));
                return (TValue)ghandle.Target;
            }

            IntPtr GetManagedPointerTo(int index)
            {
                return IntPtr(_array->GetPointer(index));
            }

            IntPtr GetManagedPointer(int index)
            {
                return IntPtr(_array->GetValue<void*>(index));
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

            void SetManagedPointer(int index, IntPtr value, int size)
            {
                _array->SetValue(index, value.ToPointer(), size);
            }

            void SetPointer(int index, void* value)
            {
                _array->SetValue(index, value);
            }

            void SetManagedPointer(int index, IntPtr value)
            {
                _array->SetValue(index, value.ToPointer());
            }

            generic<typename TValue>
            void SetValue(int index, TValue value, int size)
            {
                GCHandle handle = GCHandle::Alloc(value);
                _array->SetValue(index, GCHandle::ToIntPtr(handle).ToPointer(), size);
                handle.Free();
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
