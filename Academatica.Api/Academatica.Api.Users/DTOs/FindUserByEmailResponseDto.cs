using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Users.DTOs
{
    public class FindUserByEmailResponseDto
    {
        public bool Success { get; set; }
        public Guid? UserId { get; set; }
    }
}
