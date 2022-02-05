using System;
using System.ComponentModel.DataAnnotations;

namespace Academatica.Api.Common.Models
{
    public class UserTopicMistake
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public string TopicId { get; set; }
        [Required]
        public ulong MistakeCount { get; set; }

        public User User { get; set; }
        public Topic Topic { get; set; }
    }
}
