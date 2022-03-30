using Images.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Images.Web.Models
{
    public class ImageViewModel
    {
        public Image Image { get; set; }
        public bool CouldView { get; set; }
        public string Message { get; set; }
        
    }
}
