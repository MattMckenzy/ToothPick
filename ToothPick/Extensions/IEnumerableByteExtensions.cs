using System.Numerics;

namespace ToothPick.Extensions
{
    public static class IEnumerableByteExtensions
    {
        public static string ToBase62String(this IEnumerable<byte> bytes)
        {    
            const string alphanumericString = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            
            if (!BitConverter.IsLittleEndian) 
                bytes = bytes.Reverse();
            
            BigInteger dividend = new(bytes.ToArray());
            StringBuilder builder = new();

            while (dividend != 0)
            {
                BigInteger remainder;
                dividend = BigInteger.DivRem(dividend, 62, out remainder);
                builder.Insert(0, alphanumericString[Math.Abs(((int)remainder))]);
            }

            return builder.ToString();
        }
    }
}
