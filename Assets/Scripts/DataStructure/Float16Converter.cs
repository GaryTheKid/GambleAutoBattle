using System;

public static class Float16Converter
{
    public static ushort PackFloat(float value)
    {
        int f = BitConverter.SingleToInt32Bits(value);
        int sign = (f >> 16) & 0x8000; // Get sign bit
        int exponent = ((f >> 23) & 0xFF) - (127 - 15); // Adjust exponent bias
        int mantissa = (f & 0x007FFFFF) >> 13; // Convert mantissa to 10-bit

        if (exponent <= 0)
        {
            return (ushort)(sign | (mantissa >> -exponent)); // Handle denormals
        }
        if (exponent > 31)
        {
            return (ushort)(sign | 0x7C00); // Handle infinity/NaN
        }

        return (ushort)(sign | (exponent << 10) | mantissa);
    }

    public static float UnpackFloat(ushort half)
    {
        int sign = (half & 0x8000) << 16;
        int exponent = (half & 0x7C00) >> 10;
        int mantissa = (half & 0x03FF) << 13;

        if (exponent == 0)
        {
            if (mantissa == 0) return BitConverter.Int32BitsToSingle(sign); // Zero
            exponent = 127 - 15; // Adjust exponent
        }
        else if (exponent == 31)
        {
            return BitConverter.Int32BitsToSingle(sign | 0x7F800000 | mantissa); // Infinity/NaN
        }
        else
        {
            exponent += (127 - 15);
        }

        int packed = sign | (exponent << 23) | mantissa;
        return BitConverter.Int32BitsToSingle(packed);
    }
}