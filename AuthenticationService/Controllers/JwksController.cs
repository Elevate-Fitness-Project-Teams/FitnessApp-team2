using AuthenticationService.Options;
using AuthenticationService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AuthenticationService.Controllers;

[ApiController]
public class JwksController : ControllerBase
{
    private readonly JwtOptions _options;

    public JwksController(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    [HttpGet("/.well-known/jwks.json")]
    public IActionResult GetJwks()
    {
        var jwk = RsaKeyService.GetJsonWebKey(_options.PublicKeyPath, _options.KeyId);
        return Ok(new { keys = new[] { jwk } });
    }
}
