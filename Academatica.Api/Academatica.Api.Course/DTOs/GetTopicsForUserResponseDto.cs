using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Course.DTOs
{
    public class GetTopicsForUserResponseDto
    {
        public IEnumerable<GetTopicForUserResponseDto> Topics { get; set; }
    }
}
