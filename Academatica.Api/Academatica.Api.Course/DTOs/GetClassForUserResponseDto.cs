using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Course.DTOs
{
    public class GetClassForUserResponseDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ulong ExpReward
        {
            get
            {
                return IsComplete ? 50ul : 100ul;
            }
        }
        public string ImageUrl { get; set; }
        public string TheoryUrl { get; set; }
        public uint ProblemNum { get; set; }
        public string TopicName { get; set; }
        public bool IsComplete { get; set; }
        public bool IsUnlocked { get; set; }
    }
}
