using System;
using System.Collections.Generic;
using Academatica.Api.Common.Models;

namespace Academatica.Api.Course.DTOs
{
    public class GetUpcomingClassesResponseDto
    {
        public IEnumerable<UpcomingClassDto> UpcomingClasses { get; set; }
    }
}
