using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartMath.Api.Common.Models.Tokens
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public string JwtId { get; set; }
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        public DateTimeOffset IssuedAt { get; set; }
        public DateTimeOffset ExpiryDate { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
    }
}
