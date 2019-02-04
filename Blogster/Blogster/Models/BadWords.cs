using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace Blogster.Models
{
    public class BadWords
    {
        [Key]
        [Required]
        public int BadWordId
        {
            get;
            set;
        }

        [Required]
        [StringLength(50)]
        public string Word
        {
            get;
            set;
        }
    }
}
