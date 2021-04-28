using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rsademo
{
    class Program
    {
        public static Tuple<ulong, ulong> PublicKey;
        public static Tuple<ulong, ulong> PrivateKey;
        
        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var (p, q) = GetPrimeNumbers();
            var n = p * q;

            var phi = (p - 1) * (q - 1);
            ulong e = 65537; // prime number recommended by article
            while (Gcd(phi, e) != 1)
            {
                while (!IsPrime(e))
                {
                    e++;
                }
            }

            var d = (1 / e) % phi;

            PublicKey = new Tuple<ulong, ulong>(e, n);
            PrivateKey = new Tuple<ulong, ulong>(d, n);

            stopwatch.Stop();
            
            //Console.WriteLine($"{n1} | {n2}");
            Console.WriteLine($"key generation took {stopwatch.Elapsed}");
            
            stopwatch.Reset();

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
            Console.WriteLine($"decryption took {stopwatch.Elapsed}");
        }

        public static (ulong, ulong) GetPrimeNumbers()
        {
            var random = new Random();

            ulong n1 = (ulong) random.Next();
            ulong n2 = (ulong) random.Next();

            while (IsPrime(n1) && IsPrime(n2))
            {
                n1 = (ulong) random.Next();
                n2 = (ulong) random.Next();
            }

            return (n1, n2);
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
        
        // source:
        // https://stackoverflow.com/questions/46846973/coprime-integers
        public static ulong Gcd(ulong m, ulong n)
        {
            ulong tmp = 0;
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

        public static int Crypt(int x, ulong y, ulong n)
        {
            return (int) (Math.Pow(x, y) % n);
        }
    }
}