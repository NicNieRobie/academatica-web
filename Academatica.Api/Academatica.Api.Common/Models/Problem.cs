using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academatica.Api.Common.Models
{
    class Problem
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string ClassId { get; set; }
        [Required]
        public string TopicId { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Task { get; set; }
        [Required]
        [RegularExpression("^sc$|^mc$|^pic$|^txt$|^gap$", ErrorMessage = "Invalid problem type.")]
        public string ProblemType { get; set; }
        [Required]
        public List<string> Options { get; set; }
        public string ImageUrl { get; set; }
        [Required]
        public List<string> CorrectAnswers { get; set; }
        [RegularExpression("^$|^.*_GAP_.*$", ErrorMessage = "Expression does not contain a gap.")]
        public string Expression { get; set; }
        [Required]
        [Range(1, 3, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public ushort Difficulty { get; set; }

        public Topic Topic { get; set; }
        public Class Class { get; set; }
    }
}
