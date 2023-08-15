using BulkyBook.DataAccess.Repository.UnitOfWork;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = StaticDetails.ROLE_ADMIN)]
	public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        //Edit Action Method
        public IActionResult Upsert(int? id)
        {
            ProductViewModel productViewModel = new()
            {
                productModel = new(),
                CategoryList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                CoverTypeList = _unitOfWork.CoverType.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
            };
            if (id is null || id.Equals(0))
            {
                //create
                //ViewBag.CategoryList = CategoryList;
                //ViewData["CoverTypeList"] = CoverTypeList;
                return View(productViewModel);
            }
            else
            {
                //update
                productViewModel.productModel = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id, includeProperties:"ProductImages");
                return View(productViewModel);
            }
        }

        //POST Action Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductViewModel obj, List<IFormFile> files)
        {
            if (ModelState.IsValid)
            {
                if (obj.productModel.Id.Equals(0))
                    _unitOfWork.Product.Add(obj.productModel);
                else
                    _unitOfWork.Product.Update(obj.productModel);

                _unitOfWork.Save();

                string wwwRootPath = _hostEnvironment.WebRootPath;
                if (files is not null)
                {
                    foreach(IFormFile file in files)
                    {
                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var productPath = @"img\products\product-" + obj.productModel.Id;
                        var finalPath = Path.Combine(wwwRootPath, productPath);

                        if (!Directory.Exists(finalPath))
                            Directory.CreateDirectory(finalPath);

                        //copy files from input file
                        using (var fileStreams = new FileStream(Path.Combine(finalPath, filename), FileMode.Create))
                        {
                            file.CopyTo(fileStreams);
                        }

                        ProductImageModel productImages = new()
                        {
                            ImgUrl = $"\\{productPath}\\{filename}",
                            ProductId = obj.productModel.Id
                        };

                        if(obj.productModel.ProductImages is null)
                            obj.productModel.ProductImages = new List<ProductImageModel>();

                        obj.productModel.ProductImages.Add(productImages);
                    }

                    _unitOfWork.Product.Update(obj.productModel);
                    _unitOfWork.Save();
                }

                TempData["success"] = "Product created/updated successfully.";
                return RedirectToAction(nameof(Upsert), new { id = obj.productModel.Id });
            }
            else
            {
                obj.CategoryList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });

                obj.CoverTypeList = _unitOfWork.CoverType.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });
                return View(obj);
            }
           
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _unitOfWork.Product.GetAll(includeProperties:"Category,CoverType");
            return Json(new { data = productList });
        }
        #endregion

        //Delete Action Method
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var ProductObj = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id);

            if (ProductObj is null)
                return Json(new { success = false, message = "Error while deleting." });

            //var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, ProductObj.ImageUrl.TrimStart('\\'));
            //if (System.IO.File.Exists(oldImagePath))
            //    System.IO.File.Delete(oldImagePath);

            var productPath = @"img\products\product-" + id;
            var finalPath = Path.Combine(_hostEnvironment.WebRootPath, productPath);

            if (Directory.Exists(finalPath))
            {
                string[] filePaths = Directory.GetFiles(finalPath);

                foreach (string filePath in filePaths)
                    System.IO.File.Delete(filePath);

                Directory.Delete(finalPath);
            }

            _unitOfWork.Product.Remove(ProductObj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Product deleted successfully." });
        }

        public IActionResult DeleteImage(int imageid)
        {
            var image = _unitOfWork.ProductImages.GetFirstOrDefault(x => x.Id == imageid);
            if(image is not null)
            {
                if(!string.IsNullOrEmpty(image.ImgUrl))
                {
                    var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, image.ImgUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                _unitOfWork.ProductImages.Remove(image);
                _unitOfWork.Save();

                TempData["success"] = "Image deleted successfully.";
            }

            return RedirectToAction(nameof(Upsert), new { id = image.ProductId });
        }
    }
}