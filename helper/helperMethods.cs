using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace testingStuff.helper;

public static class HelperMethods
{

    #region textGenerationArrayHelpers
    public static int getResponsePoolIndex(string[] responsePool, string response)
    {
        int index_to_find = -1;
        foreach (string item in responsePool)
        {
            if (item.ToLower().Contains(response.ToLower()))
            {
                index_to_find = Array.IndexOf(responsePool, item);
                return index_to_find;
            }
        }
        return index_to_find;
    }

    public static bool arrayContains(string[] responsePool, string response)
    {
        foreach (string item in responsePool)
        {
            if (item.ToLower().Contains(response.ToLower()))
            {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Jwt thingies

    public static JwtSecurityToken ConvertJwtStringToJwtSecurityToken(string? jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);

        return token;
    }

    public static DecodedToken DecodeJwt(JwtSecurityToken token)
    {
        var keyId = token.Header.Kid;
        var audience = token.Audiences.ToList();
        var claims = token.Claims.Select(claim => (claim.Type, claim.Value)).ToList();
        return new DecodedToken(
            keyId,
            token.Issuer,
            audience,
            claims,
            token.ValidTo,
            token.SignatureAlgorithm,
            token.RawData,
            token.Subject,
            token.ValidFrom,
            token.EncodedHeader,
            token.EncodedPayload
        );
    }

    public static JwtSecurityToken decodeToken(/*string JwtTokenString, string notEncodedKey, */IConfiguration _config, HttpContext httpContext)
    {

        var notEncodedKey = _config["JwtSettings:Key"]!;
        var JwtTokenString = httpContext.Request.Headers.Authorization;       
        JwtTokenString = JwtTokenString.ToString().Substring("Bearer ".Length);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(notEncodedKey);
        //token = token.ToString().Substring("Bearar ".Length);
        tokenHandler.ValidateToken(JwtTokenString, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        }, out SecurityToken validatedToken);

        var jwtToken = (JwtSecurityToken)validatedToken;
        return jwtToken;
    }

    public record DecodedToken(
        string KeyId,
        string Issuer,
        List<string> Audience,
        List<(string Type, string Value)> Claims,
        DateTime Expiration,
        string SigningAlgorithm,
        string RawData,
        string Subject,
        DateTime ValidFrom,
        string Header,
        string Payload
    );
    #endregion


}