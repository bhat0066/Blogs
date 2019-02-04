using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Blogster.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Diagnostics;
using J3QQ4;

namespace Blogster.Controllers
{
    public class Home : Controller
    {

        private BlogsterDataContext _blogsterDataContext;


        public Home(BlogsterDataContext context)
        {
            _blogsterDataContext = context;
        }

        public IActionResult Index()
        {
            var indexInfo = new BlogViewModel();
            var blogInfo = (from blog in _blogsterDataContext.BlogPosts select blog).ToList();
            indexInfo.BlogsList = new List<BlogPosts>();
            indexInfo.Photo = new List<Photos>();
            foreach (var blog in blogInfo)
            {
                indexInfo.BlogsList.Add(new BlogPosts() { BlogPostID = blog.BlogPostID, Title = blog.Title, Content = blog.Content, ShortDescription = blog.ShortDescription, Posted = blog.Posted, IsAvailable = blog.IsAvailable });
                var photoURL = (from url in _blogsterDataContext.Photos where url.BlogPostId == blog.BlogPostID select url.Url).FirstOrDefault();

                indexInfo.Photo.Add(new Photos() { Url = photoURL });
            }

            return View(indexInfo);
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult VerifyUser(Users user)
        {
            var email = user.EmailAddress;
            var pass = user.Password;
            var checkUser = (from u in _blogsterDataContext.Users where (u.EmailAddress == email && u.Password == pass) select u).FirstOrDefault();

            if (checkUser == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                HttpContext.Session.SetString("FirstName", checkUser.FirstName);
                HttpContext.Session.SetString("LastName", checkUser.LastName);
                HttpContext.Session.SetInt32("UserId", checkUser.UserId);
                HttpContext.Session.SetInt32("RoleId", checkUser.RoleId);
                return RedirectToAction("Index");
            }
        }

        public IActionResult CreateUser(Users user, int selectAdmin)
        {
            if (selectAdmin == 0)
            {
                user.RoleId = 2;
            }
            else
            {
                user.RoleId = 1;
            }
            _blogsterDataContext.Users.Add(user);
            _blogsterDataContext.SaveChanges();
            return RedirectToAction("Login");
        }

        public IActionResult EditProfile(int id)
        {
            var getUser = (from user in _blogsterDataContext.Users where user.UserId == id select user).FirstOrDefault();
            return View(getUser);
        }

        public IActionResult UpdateProfile(Users user, string userid)
        {
            var idUser = Convert.ToInt32(userid);
            var userToUpdate = (from u in _blogsterDataContext.Users where u.UserId == idUser select u).FirstOrDefault();

            userToUpdate.FirstName = user.FirstName;
            userToUpdate.LastName = user.LastName;
            userToUpdate.EmailAddress = user.EmailAddress;
            userToUpdate.Password = user.Password;
            userToUpdate.DateOfBirth = user.DateOfBirth;
            userToUpdate.City = user.City;
            userToUpdate.Address = user.Address;
            userToUpdate.PostalCode = user.PostalCode;
            userToUpdate.Country = user.Country;

            _blogsterDataContext.SaveChanges();

            HttpContext.Session.SetString("FirstName", user.FirstName);
            HttpContext.Session.SetString("LastName", user.LastName);
            HttpContext.Session.SetInt32("UserId", user.UserId);

            return RedirectToAction("Index");
        }


        public IActionResult CreateComment(Comments comment, string userid, string blogpostid)
        {
            var idUser = Convert.ToInt32(userid);
            var idBlog = Convert.ToInt32(blogpostid);
            var userComment = comment.Content;
            var badWords = (from words in _blogsterDataContext.BadWords select words.Word).ToArray();

            foreach (var word in badWords)
            {
                if (userComment.Contains(word))
                {
                    userComment = userComment.Replace(word, Emoji.Dog + Emoji.Dolphin + Emoji.Eggplant);
                }
            }

            comment.UserId = idUser;
            comment.BlogPostId = idBlog;
            comment.Content = userComment;

            _blogsterDataContext.Add(comment);
            _blogsterDataContext.SaveChanges();
            return RedirectToAction("DisplayFullBlogPost", "Home", new { id = idBlog });
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult AddBlogPost()
        {
            return View();
        }

        public IActionResult AddPhotos(int id)
        {
            HttpContext.Session.SetString("BlogPostId", Convert.ToString(id));
            var blogPhotos = (from photos in _blogsterDataContext.Photos where photos.BlogPostId == id select photos).ToList();
            return View(blogPhotos);
        }

        public IActionResult CreateBlogPost(BlogPosts blog, string userid, int IsAvailable)
        {

            var id = Convert.ToInt32(userid);
            var currentTime = DateTime.Now;
            bool choice = true;

            blog.UserId = id;
            blog.Posted = currentTime;

            if (IsAvailable == 0)
            {
                choice = false;
            }
            else
            {
                choice = true;
            }

            var tempBlog = new BlogPosts() { UserId = id, Posted = currentTime, IsAvailable = choice, Content = blog.Content, ShortDescription = blog.ShortDescription, Title = blog.Title };

            _blogsterDataContext.BlogPosts.Add(tempBlog);
            _blogsterDataContext.SaveChanges();

            return RedirectToAction("AddPhotos", "Home", new { id = tempBlog.BlogPostID });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UploadFileNow(ICollection<IFormFile> files, string BlogPostId)
        {

            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=blogster;AccountKey=2pvh1PnvLjjAj8BG1fKfI05YQs49r0Vpo4Qv7HDVrUhwRY0w5jHVXKvQYUqgRLRDfuyIiyEi9qCTjvWGk1/RNA==");
            var blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference("imageblogs");
            await container.CreateIfNotExistsAsync();

            var permissions = new BlobContainerPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
            await container.SetPermissionsAsync(permissions);

            foreach (var file in files)
            {
                try
                {
                    var blockBlob = container.GetBlockBlobReference(file.FileName);
                    if (await blockBlob.ExistsAsync())
                        await blockBlob.DeleteAsync();

                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;

                        await blockBlob.UploadFromStreamAsync(memoryStream);
                    }

                    var photo = new Photos();
                    photo.Url = blockBlob.Uri.AbsoluteUri;
                    photo.Filename = file.FileName;
                    photo.BlogPostId = Convert.ToInt32(BlogPostId);

                    _blogsterDataContext.Photos.Add(photo);
                    _blogsterDataContext.SaveChanges();
                }
                catch
                {

                }
            }

            return RedirectToAction("Index");
        }
        public IActionResult DeleteFileNow(string fileName, int BlogPostId)
        {
            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=blogster;AccountKey=2pvh1PnvLjjAj8BG1fKfI05YQs49r0Vpo4Qv7HDVrUhwRY0w5jHVXKvQYUqgRLRDfuyIiyEi9qCTjvWGk1/RNA==");

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("imageblogs");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            blockBlob.DeleteAsync();

            var fileToDelete = (from f in _blogsterDataContext.Photos where (f.Filename == fileName & f.BlogPostId == BlogPostId) select f).FirstOrDefault();
            _blogsterDataContext.Remove(fileToDelete);
            _blogsterDataContext.SaveChanges();

            return RedirectToAction("Index");
        }


        public IActionResult DisplayFullBlogPost(int id)
        {
            var blogViewModel = new BlogViewModel();
            var getBlog = (from b in _blogsterDataContext.BlogPosts where b.BlogPostID == id select b).FirstOrDefault();
            var getUserInfo = (from u in _blogsterDataContext.Users where u.UserId == getBlog.UserId select u).FirstOrDefault();
            var getComments = (from c in _blogsterDataContext.Comments where c.BlogPostId == getBlog.BlogPostID select c).ToArray();
            var getPhotos = (from p in _blogsterDataContext.Photos where p.BlogPostId == getBlog.BlogPostID select p).ToArray();

            blogViewModel.User = new Models.Users() { FirstName = getUserInfo.FirstName, LastName = getUserInfo.LastName, EmailAddress = getUserInfo.EmailAddress };
            blogViewModel.Blogs = new Models.BlogPosts() { Title = getBlog.Title, Content = getBlog.Content, Posted = getBlog.Posted, BlogPostID = getBlog.BlogPostID };

            blogViewModel.Comments = new List<Comments>();
            blogViewModel.user = new List<Users>();
            foreach (var comments in getComments)
            {
                var getCommentsUserInfo = (from u in _blogsterDataContext.Users where u.UserId == comments.UserId select u).FirstOrDefault();
                blogViewModel.Comments.Add(new Comments() { UserId = comments.UserId, Content = comments.Content, Rating = comments.Rating });
                blogViewModel.user.Add(new Models.Users() { UserId = getCommentsUserInfo.UserId, FirstName = getCommentsUserInfo.FirstName, LastName = getCommentsUserInfo.LastName });

            }
            blogViewModel.Photo = new List<Photos>();
            foreach (var photo in getPhotos)
            {
                blogViewModel.Photo.Add(new Models.Photos() { Url = photo.Url });
            }
            return View(blogViewModel);
        }

        public IActionResult EditBlogPost(int id)
        {
            var getBlog = (from b in _blogsterDataContext.BlogPosts where b.BlogPostID == id select b).FirstOrDefault();
            return View(getBlog);
        }

        public IActionResult DeleteBlogPost(int id)
        {
            var getComments = (from comments in _blogsterDataContext.Comments where comments.BlogPostId == id select comments).ToList();
            foreach (var comment in getComments)
            {
                _blogsterDataContext.Comments.Remove(comment);
                _blogsterDataContext.SaveChanges();
            }

            var getPhotos = (from photos in _blogsterDataContext.Photos where photos.BlogPostId == id select photos).ToList();
            foreach (var photo in getPhotos)
            {
                DeleteFile(photo.Filename, photo.BlogPostId);

            }

            var getBlog = (from blog in _blogsterDataContext.BlogPosts where blog.BlogPostID == id select blog).FirstOrDefault();
            _blogsterDataContext.BlogPosts.Remove(getBlog);
            _blogsterDataContext.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult UpdateBlogPost(BlogPosts blog, string userid, string blogpostid)
        {
            var idBlog = Convert.ToInt32(blogpostid);
            var idUser = Convert.ToInt32(userid);
            var blogToUpdate = (from b in _blogsterDataContext.BlogPosts where b.BlogPostID == idBlog select b).FirstOrDefault();

            blogToUpdate.BlogPostID = idBlog;
            blogToUpdate.UserId = idUser;
            blogToUpdate.Posted = DateTime.Now;
            blogToUpdate.Title = blog.Title;
            blogToUpdate.Content = blog.Content;

            _blogsterDataContext.SaveChanges();

            return RedirectToAction("AddPhotos", "Home", new { id = idBlog });
        }

        public IActionResult ViewBadWords()
        {
            return View(_blogsterDataContext.BadWords.ToList());
        }

        public IActionResult AddBadWords(BadWords words)
        {
            _blogsterDataContext.BadWords.Add(words);
            _blogsterDataContext.SaveChanges();
            return RedirectToAction("ViewBadWords", "Home");
        }

        public IActionResult DeleteBadWord(int id)
        {
            var getWord = (from word in _blogsterDataContext.BadWords where word.BadWordId == id select word).FirstOrDefault();
            _blogsterDataContext.BadWords.Remove(getWord);
            _blogsterDataContext.SaveChanges();
            return RedirectToAction("ViewBadWords", "Home");
        }


        public void DeleteFile(string fileName, int BlogPostId)
        {
            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=blogster;AccountKey=2pvh1PnvLjjAj8BG1fKfI05YQs49r0Vpo4Qv7HDVrUhwRY0w5jHVXKvQYUqgRLRDfuyIiyEi9qCTjvWGk1/RNA==");

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("imageblogs");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            blockBlob.DeleteAsync();

            var fileToDelete = (from f in _blogsterDataContext.Photos where (f.Filename == fileName & f.BlogPostId == BlogPostId) select f).FirstOrDefault();
            _blogsterDataContext.Remove(fileToDelete);
            _blogsterDataContext.SaveChanges();
        }
    }
}
