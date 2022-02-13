using System;
using System.Collections.Generic;
using Academatica.Api.Common.Models;

namespace Academatica.Api.Course.DTOs
{
    public class GetUpcomingClassesResponseDto
    {
        public IEnumerable<Class> UpcomingClasses { get; set; }
    }
}
