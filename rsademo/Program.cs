using System;

namespace rsademo
{
    class Program
    {
        static void Main(string[] args)
        {
            var random = new Random();

            ulong n1 = (ulong) random.Next();
            ulong n2 = (ulong) random.Next();

            while (IsPrime(n1) && IsPrime(n2))
            {
                n1 = (ulong) random.Next();
                n2 = (ulong) random.Next();
            }
            
            Console.WriteLine($"{n1} | {n2}");
        }
        
        // source:
        // https://www.codeproject.com/questions/1076264/generating-random-prime-number-in-csharp
        public static bool IsPrime(ulong n)
        {
            var sqrt = Math.Sqrt(n);
            for (ulong i = 2; i <= sqrt; i++)
            {
                if ((n % i) == 0) return false;
            }

            return true;
        }
    }
}