using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartMath.Api.Auth.Configuration
{
    public class JwtConfig
    {
        public string Key { get; set; }
        public int Lifespan { get; set; }
    }
}
