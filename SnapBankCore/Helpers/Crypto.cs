using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SnapBank.Core.Helpers
{
    public static class Crypto
    {

        public static string GetCryptoKey(int bytelen=64)
        {
            using (RandomNumberGenerator rngCrypto = new RNGCryptoServiceProvider())
            {
                var tokenData = new byte[bytelen];
                rngCrypto.GetBytes(tokenData);

                var token = Convert.ToBase64String(tokenData);
                return token;
            }
        }

        public static string GetSecretHash(string secret,string salt)
        {
            return Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes($"{secret} {salt}")));
        }
    }
}
