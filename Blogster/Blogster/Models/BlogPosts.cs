using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blogster.Models
{
    public class BlogPosts
    {
        [Key]
        [Required]
        public int BlogPostID
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
        [StringLength(200)]
        public string Title
        {
            get;
            set;
        }

        [Required]
        [StringLength(400)]
        public string ShortDescription
        {
            get;
            set;
        }

        [Required]
        [StringLength(4000)]
        public string Content
        {
            get;
            set;
        }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Posted
        {
            get;
            set;
        }

        [Required]
        [Range(0,1, ErrorMessage ="It must be either true or false")]
        public bool IsAvailable
        {
            get;
            set;
        }
    }
}
