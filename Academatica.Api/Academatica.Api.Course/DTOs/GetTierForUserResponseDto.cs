using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Course.DTOs
{
    public class GetTierForUserResponseDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CompletionRate { get; set; }
        public bool IsComplete { get; set; }
        public bool IsUnlocked { get; set; }
    }
}
