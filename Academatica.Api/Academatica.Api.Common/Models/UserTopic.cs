using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academatica.Api.Common.Models
{
    public class UserTopic
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public Guid TopicId { get; set; }
        [Required]
        public DateTime CompletedAt { get; set; }

        public User User { get; set; }
        public Topic Topic { get; set; }
    }
}
