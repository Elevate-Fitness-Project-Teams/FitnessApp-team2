using System.Security.Cryptography;

namespace AuthenticationService.Tests;

public static class TestRsaKeyHelper
{
    private static readonly object _lock = new();
    private static string? _tempPrivateKeyPath;
    private static string? _tempPublicKeyPath;

    public static (string PrivateKeyPath, string PublicKeyPath) EnsureTestKeys()
    {
        lock (_lock)
        {
            if (_tempPrivateKeyPath != null && File.Exists(_tempPrivateKeyPath) &&
                _tempPublicKeyPath != null && File.Exists(_tempPublicKeyPath))
            {
                return (_tempPrivateKeyPath, _tempPublicKeyPath);
            }

            // Check if actual keys exist on disk
            var candidatePrivate = FindExistingKey("auth-service-private.pem");
            var candidatePublic = FindExistingKey("auth-service-public.pem");

            if (candidatePrivate != null && candidatePublic != null)
            {
                _tempPrivateKeyPath = candidatePrivate;
                _tempPublicKeyPath = candidatePublic;
                return (_tempPrivateKeyPath, _tempPublicKeyPath);
            }

            // In CI / GitHub Actions where Keys/ is gitignored, generate valid 2048-bit RSA keys dynamically
            var keyDir = Path.Combine(AppContext.BaseDirectory, "test-keys");
            Directory.CreateDirectory(keyDir);
            _tempPrivateKeyPath = Path.Combine(keyDir, "auth-service-private.pem");
            _tempPublicKeyPath = Path.Combine(keyDir, "auth-service-public.pem");

            if (!File.Exists(_tempPrivateKeyPath) || !File.Exists(_tempPublicKeyPath))
            {
                using var rsa = RSA.Create(2048);
                var privPem = rsa.ExportPkcs8PrivateKeyPem();
                var pubPem = rsa.ExportSubjectPublicKeyInfoPem();

                File.WriteAllText(_tempPrivateKeyPath, privPem);
                File.WriteAllText(_tempPublicKeyPath, pubPem);
            }

            return (_tempPrivateKeyPath, _tempPublicKeyPath);
        }
    }

    private static string? FindExistingKey(string filename)
    {
        var candidates = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "keys", filename),
            Path.Combine(Directory.GetCurrentDirectory(), "Keys", filename),
            Path.Combine(Directory.GetCurrentDirectory(), "../Keys", filename),
            Path.Combine(Directory.GetCurrentDirectory(), "../keys", filename),
            Path.Combine(AppContext.BaseDirectory, "keys", filename),
            Path.Combine(AppContext.BaseDirectory, "Keys", filename),
            Path.Combine(AppContext.BaseDirectory, "../../../keys", filename),
            Path.Combine(AppContext.BaseDirectory, "../../../Keys", filename),
            Path.Combine(AppContext.BaseDirectory, "../../../../keys", filename),
            Path.Combine(AppContext.BaseDirectory, "../../../../Keys", filename)
        };

        return candidates.FirstOrDefault(File.Exists);
    }
}
