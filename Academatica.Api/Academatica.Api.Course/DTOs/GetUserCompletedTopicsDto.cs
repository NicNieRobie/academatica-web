﻿using Academatica.Api.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Course.DTOs
{
    public class GetUserCompletedTopicsDto
    {
        public IEnumerable<TopicDto> CompletedTopics { get; set; }
    }
}
