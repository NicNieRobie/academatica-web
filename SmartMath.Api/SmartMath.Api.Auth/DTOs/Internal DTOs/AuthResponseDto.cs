﻿using System;
using System.Collections.Generic;

namespace SmartMath.Api.Auth.DTOs
{
    public class AuthResponseDto
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public bool Success { get; set; }
        public List<string> Errors { get; set; }
    }
}