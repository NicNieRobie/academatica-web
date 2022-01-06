using SmartMath.Api.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartMath.Api.Users.DBOs
{
    public class GetUserTiersResponseDbo
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public List<UserStatsTier> TierStats { get; set; }
    }
}
