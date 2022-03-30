using Images.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Images.Data;
using System.IO;
using Newtonsoft.Json;

namespace Images.Web.Controllers
{
    public class HomeController : Controller
    {
        private string _connectionString = "Data Source=.\\sqlexpress;Initial Catalog=Images;Integrated Security=True;Encrypt=false;";

        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upload(Image image, IFormFile uploadedImage)
        {
            string fileName = $"{Guid.NewGuid()}-{uploadedImage.FileName}";

            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName);
            using var fs = new FileStream(filePath, FileMode.CreateNew);
            uploadedImage.CopyTo(fs);

            Image im = new Image
            {
                Name = uploadedImage.FileName,
                Password = image.Password
            };

            Manager repo = new Manager(_connectionString);
            repo.Add(im);

            ImageViewModel vm = new ImageViewModel { Image = im };

            return View(vm);

        }

        [HttpPost]
        public IActionResult viewImage(int id, string password)
        {
            ImageViewModel vm = new ImageViewModel();
            Manager repo = new Manager(_connectionString);
            var image = repo.GetImage(id);
            if (password == image.Password)
            {
                var allowedIds = HttpContext.Session.Get<List<int>>("allowedIds");
                if (allowedIds == null)
                {
                    allowedIds = new List<int>();
                }
                allowedIds.Add(id);
                HttpContext.Session.Set("allowedids", allowedIds);
                vm.Image = image;
                
            }

            return View(vm);
        }



        public IActionResult viewimage(int id)
        {

            ImageViewModel vm = new ImageViewModel();


            if (!HasPermissionToView(id))
            {
                vm.CouldView = false;
                //vm.Image = new Image { Id = id };
            }
            else
            {
                vm.CouldView = true;
                var db = new Manager(_connectionString);
                db.IncrementViewCount(id);
                var image = db.GetImage(id);
                if (image == null)
                {
                    return RedirectToAction("Index");
                }

                vm.Image = image;
            }

            return View(vm);
        }

        private bool HasPermissionToView(int id)
        {
            var allowedIds = HttpContext.Session.Get<List<int>>("allowedids");
            if (allowedIds == null)
            {
                return false;
            }

            return allowedIds.Contains(id);
        }

    }
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            string value = session.GetString(key);

            return value == null ? default(T) :
                JsonConvert.DeserializeObject<T>(value);
        }
    }
}
