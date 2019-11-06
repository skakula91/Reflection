using System;

namespace TestObjectGenerator
{
    public static class GenerateDefault
    {
        public static Int32 GetIntValue()
        {
            return new Random().Next(1, 10000);
        }

        public static Int64 GetLongValue()
        {
            return new Random().Next(1, 10000);
        }
    }
}
