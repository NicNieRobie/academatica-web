using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Course.DTOs
{
    public class GetCustomPracticeProblemsRequestDto
    {
        public Dictionary<string, TopicProblemsDataDto> TopicData { get; set; }
    }
}
