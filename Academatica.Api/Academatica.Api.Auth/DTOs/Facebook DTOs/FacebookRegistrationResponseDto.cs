using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartMath.Api.Auth.DTOs
{
    public class FacebookRegistrationResponseDto : RegistrationResponseDto
    {
        public Guid AssociatedID { get; set; }
    }
}
