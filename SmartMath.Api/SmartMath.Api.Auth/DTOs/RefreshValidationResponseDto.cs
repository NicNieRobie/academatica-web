using SmartMath.Api.Auth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartMath.Api.Auth.DTOs
{
    public class RefreshValidationResponseDto
    {
        public bool IsValid { get; set; }
        public string Error { get; set; }
        public RefreshToken InvalidatedToken { get; set; }
    }
}
