using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Users.DTOs
{
    public class ExpChangePublishDto
    {
        public Guid UserId { get; set; }
        public ulong ExpThisWeek { get; set; }
        public string Event { get; set; }
    }
}
