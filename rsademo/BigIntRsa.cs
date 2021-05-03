using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace rsademo
{
    public class BigIntRsa
    {
        public int KeySize = 1024;

        public BigInteger P;
        public BigInteger Q;
        public BigInteger N;
        public BigInteger Phi;
        public BigInteger Egg;
        public BigInteger D;

        private readonly int rMillerConfidence = 20;

        public async Task Prepare()
        {
            await GeneratePrimes();
            CalculateN();
            CalculatePhi();
            ChooseE();
            CalculateD();

            var test = new BigInteger(69);

            var encrypted = Encrypt(test);
            var decrypted = Decrypt(encrypted);

            if (decrypted != test)
            {
                throw new Exception($"Encryption test failed. Expected 69 but got {decrypted}");
            }
        }

        public BigInteger Encrypt(BigInteger pText)
        {
            var exponent = new BigInteger(Egg);
            var modulo = new BigInteger(N);

            return pText.modPow(exponent, modulo);
        }

        public BigInteger Decrypt(BigInteger cText)
        {
            var exponent = new BigInteger(D);
            var modulo = new BigInteger(N);

            return cText.modPow(exponent, modulo);
        }

        private async Task GeneratePrimes()
        {
            var pLength = (uint) (KeySize / 2);
            var qLength = (uint) KeySize - pLength;

            P = GenerateOddNumber(pLength);
            Q = GenerateOddNumber(qLength);
            
            var tasks = new List<Task>
            {
                Task.Run(() =>
                {
                    while (!RMillerPrimeTest(P, rMillerConfidence))
                    {
                        P = GenerateOddNumber(pLength);
                    }
                }),
                Task.Run((() =>
                {
                    while (!RMillerPrimeTest(Q, rMillerConfidence))
                    {
                        Q = GenerateOddNumber(qLength);
                    }
                }))
            };


            await Task.WhenAll(tasks);

            if (P >= Q) return;
            var tmp = new BigInteger(this.P);
            this.P = this.Q;
            this.Q = tmp;
        }

        private void CalculateN()
        {
            N = P * Q;
        }

        private void CalculatePhi()
        {
            Phi = (P - 1) * (Q - 1);
        }

        private void ChooseE()
        {
            var egg = GenerateOddNumber((uint) (KeySize / 2));

            while (!AreRelativePrime(Phi, egg))
            {
                egg++;
            }

            Egg = egg;
        }

        private void CalculateD()
        {
            D = ModInv(Egg, Phi);
        }

        private bool AreRelativePrime(BigInteger phi, BigInteger egg)
        {
            return GreatestCommonDivisor(phi, egg) == 1;
        }
        
        // totally didnt ctrl-c ctrl-v this, nahhhhhh
        private BigInteger GreatestCommonDivisor(BigInteger x, BigInteger y)
        {
            BigInteger tmp;

            if (x < y)
            {
                tmp = x;
                x = y;
                y = tmp;
            }

            while (true)
            {
                tmp = x % y;
                x = y;
                y = tmp;

                if (y == 0) break;
            }

            // This will be the GCD
            return x;
        }

        private BigInteger GenerateOddNumber(uint nBits)
        {
            var result = new BigInteger();
            var random = new Random();

            for (uint i = 0; i < nBits; i++)
            {
                if (random.Next(2) == 1) result.setBit(i);
            }
            
            // ensures the bigint is odd
            result.setBit(0);
            
            // ensures ns high bit is set
            result.setBit(nBits-1);
            result.setBit(nBits-2);

            return result;
        }

        private bool RMillerPrimeTest(BigInteger n, int confidence)
        {
            var result = false;
            
            // cannot be even or less than 4
            if (n % 2 == 0 || n < 4) return result;

            var random = new Random();
            uint randNBits = 0;
            int bitCount;
            var n1 = n - new BigInteger(1);
            var n2 = n - new BigInteger(2);
            BigInteger x, d, s, a;
            bitCount = n2.bitCount();

            var tmp2 = Get2sdFromNminus1(n);
            s = tmp2[0];
            d = tmp2[1];

            while (confidence-- > 0)
            {
                while (true)
                {
                    while (randNBits < 2)
                    {
                        randNBits = (uint) (random.NextDouble() * bitCount);
                    }

                    a = GenerateOddNumber(randNBits);
                    
                    if (a < n2 || a != 0) break;
                }
                
                x = a.modPow(d, n); // this is much slower than a.modPow(d,n) from BigInteger class, replaced on 02/01/2011
                result = false;
                if (x == 1 || x == n1) { result = true; continue; }

                for (BigInteger r = 0; r < s; r += 1)
                {
                    x = (x * x) % n;
                    if (x == 1) { result = false; break; }
                    if (x == n1) { result = true; break; } 
                }

                if (result == false) break;
            }

            return result;
        }
        
        private BigInteger[] Get2sdFromNminus1(BigInteger n)
        {
            // Even number passed
            if (n % 2 == 0) return new BigInteger[] { 0, 0 };

            BigInteger tmp = n - new BigInteger(1);
            Int32 counter = 0;
            BigInteger remainder;

            while (true)
            {
                tmp = tmp / 2;
                remainder = tmp % 2;
                counter++;

                // if remainder is different from 0, then we reached an odd number
                if (remainder != 0) break;
            }

            // counter is s, tmp is d, odd
            return new[] { counter, tmp };
        }
        
        private BigInteger ModInv(BigInteger a, BigInteger b)
        {
            BigInteger x, prevX, _b, y, prevY, _a, quotient, temp;

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
    }
}