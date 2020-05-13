#include "pch.h"
#include "ByteArray.h"

ByteArray::ByteArray(int size)
{
    _array = new unsigned char[size];
    _size = size;
}

ByteArray::ByteArray(ByteArray* copy)
{
    _array = new unsigned char[copy->_size];
    _size = copy->_size;
    memcpy(_array, copy, _size);
}

ByteArray::~ByteArray()
{
    delete _array;
}

void* ByteArray::GetPointer(int index)
{
    return &_array[index];
}

unsigned char ByteArray::GetByteValue(int index)
{
    return _array[index];
}

void ByteArray::SetValue(int index, void* value, int size)
{
    memcpy(&_array[index], value, size);
}

void ByteArray::SetByteValue(int index, unsigned char value)
{
    _array[index] = value;
}

int ByteArray::Length()
{
    return _size;
}

void ByteArray::CopyTo(unsigned char* dest, int destIndex, int srcIndex, int length)
{
    memcpy(&dest[destIndex], &_array[srcIndex], length);
}

template<class TValue>
TValue* ByteArray::GetValue(int index)
{
    return (TValue*)&(_array[index]);
}

template<class TValue>
void ByteArray::SetValue(int index, TValue value)
{
    TValue* ptr = &_array[index];
    *ptr = value;
}

template<class TValue>
void ByteArray::SetValueByPointer(int index, TValue* value)
{
    TValue* ptr = &_array[index];
    *ptr = *value;
}
