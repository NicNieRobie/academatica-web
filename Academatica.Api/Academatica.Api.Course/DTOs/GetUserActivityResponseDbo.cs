using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Course.DTOs
{
    public class GetUserActivityResponseDbo
    {
        public Dictionary<DateTime, int> ActivityMatrix { get; set; }
    }
}
