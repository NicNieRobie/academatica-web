using Academatica.Api.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Course.DTOs
{
    public class UpcomingClassDto
    {
        public string Id { get; set; }
        public string TopicId { get; set; }
        public string TierId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ulong ExpReward { get; set; }
        public string ImageUrl { get; set; }
        public string TheoryUrl { get; set; }
        public uint ProblemNum { get; set; }
        public bool IsAlgebraClass { get; set; }
        public string TopicName { get; set; }
        public int ClassNumber { get; set; }
        public int TopicClassCount { get; set; }
    }
}
