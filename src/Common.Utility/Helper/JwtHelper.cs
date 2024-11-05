using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Common.Utility.Helper
{
    /// <summary>
    /// 用于 JWT 令牌生成、验证和刷新的帮助类。
    /// </summary>
    public class JwtHelper
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenExpirationMinutes;
        private readonly int _refreshTokenExpirationDays;

        /// <summary>
        /// 初始化 JwtHelper 类的新实例。
        /// </summary>
        /// <param name="secretKey">用于令牌生成的密钥。</param>
        /// <param name="issuer">令牌的发行者。</param>
        /// <param name="audience">令牌的受众。</param>
        /// <param name="accessTokenExpirationMinutes">访问令牌的过期时间（分钟）。</param>
        /// <param name="refreshTokenExpirationDays">刷新令牌的过期时间（天）。</param>
        public JwtHelper(string secretKey, string issuer, string audience, int accessTokenExpirationMinutes, int refreshTokenExpirationDays)
        {
            if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
            {
                throw new ArgumentException("密钥必须至少为 32 字节（256 位）", nameof(secretKey));
            }
            _secretKey = secretKey;
            _issuer = issuer;
            _audience = audience;
            _accessTokenExpirationMinutes = accessTokenExpirationMinutes;
            _refreshTokenExpirationDays = refreshTokenExpirationDays;
        }

        /// <summary>
        /// 生成访问令牌。
        /// </summary>
        /// <param name="userId">用户ID。</param>
        /// <returns>生成的访问令牌。</returns>
        public string GenerateAccessToken(string userId)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, userId) // 可以根据需要添加更多用户信息
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _issuer,
                _audience,
                claims,
                expires: DateTime.Now.AddMinutes(_accessTokenExpirationMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// 生成刷新令牌。
        /// </summary>
        /// <returns>生成的刷新令牌。</returns>
        public string GenerateRefreshToken()
        {
            // 这里可以使用更复杂的生成逻辑，比如随机字符串或 GUID,保存到指定为止，设置有效期
            //    _refreshTokenExpirationDays;
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 验证访问令牌。
        /// </summary>
        /// <param name="token">要验证的访问令牌。</param>
        /// <returns>包含验证结果的 ClaimsPrincipal 对象。</returns>
        public ClaimsPrincipal ValidateAccessToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);
            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                return tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            }
            catch (Exception)
            {
                return null; // 处理验证失败的情况
            }
        }

        /// <summary>
        /// 刷新访问令牌。
        /// </summary>
        /// <param name="refreshToken">刷新令牌。</param>
        /// <param name="userId">用户ID。</param>
        /// <returns>生成的新访问令牌。</returns>
        public string RefreshAccessToken(string refreshToken, string userId)
        {
            // 验证刷新令牌的有效性（这里可以根据业务逻辑来处理，通常需要查数据库）
            // 如果有效，生成新的访问令牌
            var newAccessToken = GenerateAccessToken(userId);
            return newAccessToken;
        }

        /// <summary>
        /// 验证刷新令牌。
        /// </summary>
        /// <param name="refreshToken">要验证的刷新令牌。</param>
        /// <returns>如果刷新令牌有效，则为 true；否则为 false。</returns>
        public bool ValidateRefreshToken(string refreshToken)
        {
            // 实际实现中需要检查刷新令牌是否存在于数据库或其他存储中
            return !string.IsNullOrEmpty(refreshToken);
        }
    }
}
