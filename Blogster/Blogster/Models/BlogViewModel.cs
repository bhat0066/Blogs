using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blogster.Models
{
    public class BlogViewModel
    {

        public Users User
        {
            get;
            set;
        }

        public List<Photos> Photo
        {
            get;
            set;
        }

        public BlogPosts Blogs
        {
            get;
            set;
        }

        public List<BlogPosts> BlogsList
        {
            get;
            set;
        }
        public List<Comments> Comments
        {
            get;
            set;
        }

        public List<Users> user { get; set; }
    }
}

