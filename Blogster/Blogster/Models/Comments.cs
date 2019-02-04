using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blogster.Models
{
    public class Comments
    {
        [Key]
        [Required]
        public int CommentId
        {
            get;
            set;
        }

        [Required]
        public int BlogPostId
        {
            get;
            set;
        }

        [Required]
        public int UserId
        {
            get;
            set;
        }

        [Required]
        [StringLength(2048)]
        public string Content
        {
            get;
            set;
        }

        [Required]
        [Range(0, 5, ErrorMessage = "Ratings can't exceed 5 or be less then 5")]
        public int Rating
        {
            get;
            set;
        }
    }
}
