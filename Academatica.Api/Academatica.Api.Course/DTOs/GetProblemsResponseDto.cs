﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Course.DTOs
{
    public class GetProblemsResponseDto
    {
        public IEnumerable<ProblemDto> Problems { get; set; }
    }
}