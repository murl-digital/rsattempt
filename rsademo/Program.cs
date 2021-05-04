using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ShellProgressBar;

namespace rsademo
{
    class Program
    {
        public static Tuple<long, long> PublicKey;
        public static Tuple<long, long> PrivateKey;
        
        static async Task Main(string[] args)
        {
            var rsa = new BigIntRsa();

            try
            {
                var options = new ProgressBarOptions
                {
                    ProgressCharacter = '#',
                };

                using (var bar = new ProgressBar(1, "Preparing...", options))
                {
                    await rsa.Prepare();
                    bar.Tick();
                }
                
                Console.WriteLine($"Public key: [{rsa.Egg}, {rsa.N}]");
                Console.WriteLine($"Private key: [{rsa.D}, {rsa.N}]");

                //var TestMessage = "hee hoo pee nut";
                //var TestMessage = "One of the basic pieces of furniture, a chair is a type of seat. Its primary features are two pieces of a durable material, attached as back and seat to one another at a 90° or slightly greater angle, with usually the four corners of the horizontal seat attached in turn to four legs—or other parts of the seat's underside attached to three legs or to a shaft about which a four-arm turnstile on rollers can turn—strong enough to support the weight of a person who sits on the seat (usually wide and broad enough to hold the lower body from the buttocks almost to the knees) and leans against the vertical back (usually high and wide enough to support the back to the shoulder blades). The legs are typically high enough for the seated person's thighs and knees to form a 90° or lesser angle.[1][2] Used in a number of rooms in homes (e.g. in living rooms, dining rooms, and dens), in schools and offices (with desks), and in various other workplaces, chairs may be made of wood, metal, or synthetic materials, and either the seat alone or the entire chair may be padded or upholstered in various colors and fabrics. Chairs vary in design. An armchair has armrests fixed to the seat;[3] a recliner is upholstered and under its seat is a mechanism that allows one to lower the chair's back and raise into place a fold-out footrest;[4] a rocking chair has legs fixed to two long curved slats; and a wheelchair has wheels fixed to an axis under the seat.[5]";

                var builder = new StringBuilder();
                
                foreach (var file in Directory.EnumerateFiles("shonkspeare"))
                {
                    builder.Append(await File.ReadAllTextAsync(file));
                }

                var TestMessage = builder.ToString();
                
                var encoded = new List<int>();

                using (var bar = new ProgressBar(1, "Encoding...", options))
                {
                    foreach (var b in Encoding.Unicode.GetBytes(TestMessage))
                    {
                        encoded.Add(b);
                    }
                    bar.Tick();
                }
                
                var encrypted = new BigInteger[encoded.Count];


                using (var bar = new ProgressBar(encoded.Count, "Encrypting...", options))
                {
                    Parallel.For(0, encoded.Count, i =>
                    {
                        encrypted[i] = rsa.Encrypt(new BigInteger(encoded[i]));
                        bar.Tick();
                    });
                }

                /*foreach (var i in encoded)
                {
                    encrypted.Add(rsa.Encrypt(new BigInteger(i)));
                }*/

                var decrypted = new byte[encrypted.Length];
                string decoded;

                using (var bar = new ProgressBar(encrypted.Length, "Decrypting...", options))
                {
                    Parallel.For(0, encrypted.Length, i =>
                    {
                        decrypted[i] = (byte) rsa.Decrypt(encrypted[i]).IntValue();
                        bar.Tick();
                    });
                }
                
                /*foreach (var i in encrypted)
                {
                    decrypted.Add((byte) rsa.Decrypt(i).IntValue());
                }*/

                using (var bar = new ProgressBar(1, "Decoding...", options))
                {
                    decoded = Encoding.Unicode.GetString(decrypted.ToArray());
                    bar.Tick();
                }

                Console.WriteLine(decoded);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

            /*var stopwatch = new Stopwatch();
            stopwatch.Start();

            var (p, q) = GetPrimeNumbers();

            if (p < q)
            {
                var temp = p;
                p = q;
                q = temp;
            }
            
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
            Console.WriteLine($"after: {test3}");*/

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

            return x.modPow(bigY, bigN);
        }
    }
}