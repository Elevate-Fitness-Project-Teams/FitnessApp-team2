using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationService.Services;

public static class RsaKeyService
{
    private static RSA? _privateKey;
    private static RSA? _publicKey;
    private static readonly object _lock = new();

    public static RsaSecurityKey GetPrivateKey(string pemFilePath, string keyId)
    {
        if (_privateKey == null)
        {
            lock (_lock)
            {
                if (_privateKey == null)
                {
                    _privateKey = RSA.Create();
                    _privateKey.ImportFromPem(File.ReadAllText(pemFilePath));
                }
            }
        }
        return new RsaSecurityKey(_privateKey) { KeyId = keyId };
    }

    public static RsaSecurityKey GetPublicKey(string pemFilePath, string keyId)
    {
        if (_publicKey == null)
        {
            lock (_lock)
            {
                if (_publicKey == null)
                {
                    _publicKey = RSA.Create();
                    _publicKey.ImportFromPem(File.ReadAllText(pemFilePath));
                }
            }
        }
        return new RsaSecurityKey(_publicKey) { KeyId = keyId };
    }

    public static JsonWebKey GetJsonWebKey(string pemFilePath, string keyId)
    {
        var rsaKey = GetPublicKey(pemFilePath, keyId);
        var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(rsaKey);
        jwk.Use = "sig";
        jwk.Alg = SecurityAlgorithms.RsaSha256;
        jwk.Kid = keyId;
        return jwk;
    }
}
