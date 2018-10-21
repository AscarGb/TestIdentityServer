using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Cryptography;

namespace SigningCredential
{
    public class RSA
    {
        public static RsaSecurityKey GenerateRsaKeys()
        {
            RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider(2048);
            return new RsaSecurityKey(rSACryptoServiceProvider);
        }
    }
}