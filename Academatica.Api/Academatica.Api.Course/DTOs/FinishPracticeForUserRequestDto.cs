using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Course.DTOs
{
    public class FinishPracticeForUserRequestDto
    {
        [Required]
        public int MistakeCount { get; set; }
        [Required]
        public bool IsCustomPractice { get; set; }
        public string TopicId { get; set; }
        public string ClassId { get; set; }
    }
}
