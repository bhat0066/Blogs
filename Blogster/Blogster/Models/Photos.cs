using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blogster.Models
{
    public class Photos
    {
        [Key]
        [Required]
        public int PhotoId
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
        [StringLength(255)]
        public string Filename
        {
            get;
            set;
        }

        [Required]
        [StringLength(2048)]
        public string Url
        {
            get;
            set;
        }
    }
}
