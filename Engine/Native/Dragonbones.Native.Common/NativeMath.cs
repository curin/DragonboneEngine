using System;
using System.Runtime.CompilerServices;

namespace Dragonbones.Native
{
    public unsafe static class NativeMath
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToInt(bool value)
        {
            return *(int*)&value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToFloat(bool value)
        {
            return *(float*)&value;
        }
    }
}
