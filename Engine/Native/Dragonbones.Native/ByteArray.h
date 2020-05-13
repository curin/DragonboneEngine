#pragma once

class __declspec(dllexport) ByteArray
{
    unsigned char* _array;
    int _size;
public:
    ByteArray(int size);
    ByteArray(ByteArray* copy);
    ~ByteArray();
    void* GetPointer(int index);
    unsigned char GetByteValue(int index);
    template<class TValue>
    TValue* GetValue(int index);
    void SetValue(int index, void* value, int size);
    void SetByteValue(int index, unsigned char value);
    template<class TValue>
    void SetValue(int index, TValue value);
    template<class TValue>
    void SetValueByPointer(int index, TValue* value);
    int Length();

    void CopyTo(unsigned char* dest, int destIndex, int srcIndex, int length);
};
