using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DecentraCloud.API.Middlewares
{
    public class NodeAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _secretKey;

        public NodeAuthenticationMiddleware(RequestDelegate next, string secretKey)
        {
            _next = next;
            _secretKey = secretKey;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                AttachNodeToContext(context, token);

            await _next(context);
        }

        private void AttachNodeToContext(HttpContext context, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var nodeId = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(nodeId))
                {
                    context.Items["Node"] = nodeId;
                }
            }
            catch
            {
                // Do nothing if JWT validation fails
                // Node won't be attached to context, so the request won't have access to secure routes
            }
        }

    }
}
