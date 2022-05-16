using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Course.DTOs
{
    public class GetUserActivityResponseDto
    {
        public Dictionary<DateTime, int> ActivityMatrix { get; set; }
    }
}
