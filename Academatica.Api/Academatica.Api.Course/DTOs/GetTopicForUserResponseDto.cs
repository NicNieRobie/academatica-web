using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Course.DTOs
{
    public class GetTopicForUserResponseDto : TopicDto
    {
        public bool IsComplete { get; set; }
        public bool IsUnlocked { get; set; }
    }
}
