using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace rsademo
{
    class Program
    {
        public static Tuple<long, long> PublicKey;
        public static Tuple<long, long> PrivateKey;
        
        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var (p, q) = GetPrimeNumbers();
            var n = p * q;

            var phi = (p - 1) * (q - 1);
            long e = 65537; // prime number recommended by article
            while (Gcd(phi, e) != 1)
            {
                if (Gcd(e, phi) == 1)
                    break;
                if (e < phi)
                    e++;
            }

            long d;
            long tempD;

            for (var i = 2;; i++)
            {
                d = (phi * i + 1) / e;
                tempD = (phi * i + 1) % e;

                if (tempD == 0)
                {
                    break;
                }
            }

            PublicKey = new Tuple<long, long>(e, n);
            PrivateKey = new Tuple<long, long>(d, n);

            stopwatch.Stop();
            
            //Console.WriteLine($"{n1} | {n2}");
            Console.WriteLine($"key generation took {stopwatch.Elapsed}");

            var test = new BigInteger(69);
            var test2 = Crypt(test, PublicKey.Item1, PublicKey.Item2);
            var test3 = Crypt(test, PrivateKey.Item1, PrivateKey.Item2);
            
            Console.WriteLine($"before: {test}");
            Console.WriteLine($"encrypted: {test2}");
            Console.WriteLine($"after: {test3}");

            /*stopwatch.Reset();

            var TestMessage = "hee hoo pee nut";
            
            stopwatch.Start();

            var encoded = new List<int>();
            var encrypted = new List<int>();

            foreach (var s in TestMessage.ToCharArray())
            {
                var bytes = Encoding.Unicode.GetBytes(s.ToString());

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);

                if (bytes.Length < 4)
                {
                    var list = new List<byte>(bytes);
                    while (list.Count < 4)
                    {
                        list.Insert(0, 0);
                    }

                    bytes = list.ToArray();
                }

                encoded.Add(BitConverter.ToInt32(bytes, 0));
                Console.WriteLine(s);
            }

            foreach (var i in encoded)
            {
                encrypted.Add(Crypt(i, PublicKey.Item1, PublicKey.Item2));
            }
            
            stopwatch.Stop();
            
            Console.WriteLine($"encryption took {stopwatch.Elapsed}");
            
            stopwatch.Reset();

            var decrypted = "";
            
            stopwatch.Start();
            
            foreach (var i in encrypted)
            {
                var j = Crypt(i, PrivateKey.Item1, PrivateKey.Item2);

                var bytes = BitConverter.GetBytes(j);
                //Console.WriteLine(bytes.Length);
                
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);

                decrypted += Encoding.Unicode.GetString(bytes);
                //Console.WriteLine(Encoding.Unicode.GetString(bytes));
            }
            
            stopwatch.Stop();
            
            Console.WriteLine(decrypted);
            Console.WriteLine($"decryption took {stopwatch.Elapsed}");*/
        }

        public static (long, long) GetPrimeNumbers()
        {
            var random = new Random();

            long n1 = (long) random.Next();
            long n2 = (long) random.Next();

            while (IsPrime(n1) && IsPrime(n2))
            {
                n1 = (long) random.Next();
                n2 = (long) random.Next();
            }

            return (n1, n2);
        }
        
        // source:
        // https://www.codeproject.com/questions/1076264/generating-random-prime-number-in-csharp
        public static bool IsPrime(long n)
        {
            var sqrt = Math.Sqrt(n);
            for (long i = 2; i <= sqrt; i++)
            {
                if ((n % i) == 0) return false;
            }

            return true;
        }
        
        // source:
        // https://stackoverflow.com/questions/46846973/coprime-integers
        public static long Gcd(long m, long n)
        {
            long tmp = 0;
            if (m < n)
            {
                tmp = m;
                m = n;
                n = tmp;
            }
            while (n != 0)
            {
                tmp = m % n;
                m = n;
                n = tmp;
            }
            return m;
        }

        // a portion of Euclid's Extended Algorithm for getting the mod inverse.
        public static long ModInv(long a, long b)
        {
            long x, prevX, _b, y, prevY, _a, quotient, temp;

            if (a < b)
            {
                x = 0;
                prevX = 1;
                _b = b;
                y = 1;
                prevY = 0;
                _a = a;
            }
            else
            {
                x = a;
                prevX = 0;
                _b = a;
                y = 0;
                prevY = 1;
                _a = b;
            }

            while (_a > 0)
            {
                temp = _a;
                quotient = _b / _a;

                _a = _b % temp;
                _b = temp;
                temp = y;

                y = prevY - quotient * temp;
                prevY = temp;
            }

            if (prevY < 0) prevY = (prevY + b) % b;
            return prevY;
        }

        public static BigInteger Crypt(BigInteger x, long y, long n)
        {
            var bigY = new BigInteger(y);
            var bigN = new BigInteger(n);
            
            return BigInteger.ModPow(x, bigY, bigN);
        }
    }
}