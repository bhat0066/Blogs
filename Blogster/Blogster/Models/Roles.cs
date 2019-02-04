using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blogster.Models
{
    public class Roles
    {
        [Key]
        [Required]
        public int RoleId
        {
            get;
            set;
        }

        [Required]
        [StringLength(50)]
        public string Name
        {
            get;
            set;
        }
    }
}
