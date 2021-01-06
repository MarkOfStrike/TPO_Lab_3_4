using System;
using System.Collections.Generic;
using System.Text;

namespace SemanticVersion
{
    public static class IntExtension
    {
        /// <summary>
        /// Получает количество цифр в указанном числе.
        /// </summary>
        public static int Digits(this int n)
        {
            if (n < 10) return 1;
            if (n < 100) return 2;
            if (n < 1_000) return 3;
            if (n < 10_000) return 4;
            if (n < 100_000) return 5;
            if (n < 1_000_000) return 6;
            if (n < 10_000_000) return 7;
            if (n < 100_000_000) return 8;
            if (n < 1_000_000_000) return 9;
            return 10;
        }
    }
}
