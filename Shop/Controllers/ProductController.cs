using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using Shop.Models.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Shop.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            IEnumerable<Product> productList = _context.Product.Include(p => p.Category);
            return View(productList);
        }

        [HttpGet]
        public IActionResult CreateEdit(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategorySelectList = _context.Category.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                })
            };

            if(id == null)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _context.Product.Find(id);
                if(productVM.Product == null)
                {
                    return NotFound();
                }
                return View(productVM);
            }
        }
        [HttpPost]
        public IActionResult CreateEdit(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;
                if(productVM.Product.Id == 0)
                {
                    string upload = webRootPath + ENV.ImagePath;
                    string fileName = Guid.NewGuid().ToString();
                    string extantion = Path.GetExtension(files[0].FileName);

                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extantion), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    };

                    productVM.Product.Image = fileName + extantion;
                    _context.Product.Add(productVM.Product);
                    _context.SaveChanges();
                }
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }
            Product product = _context.Product.Include(c => c.Category).FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteProduct(int? id)
        {
            var product = _context.Product.Find(id);
            if(product == null)
            {
                return NotFound();
            }
            string upload = _webHostEnvironment.WebRootPath + ENV.ImagePath;
            var oldImage = Path.Combine(upload, product.Image);

            if (System.IO.File.Exists(oldImage))
            {
                System.IO.File.Delete(oldImage);
            }

            _context.Remove(product);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
