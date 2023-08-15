using BulkyBook.DataAccess.Repository.Generic;
using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository.Product
{
    public class ProductImagesRepository : GenericRepository<ProductImageModel>, IProductImagesRepository
    {
        private ApplicationDBContext _db;
        public ProductImagesRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
        }

        void IProductImagesRepository.Update(ProductImageModel obj)
        {
            _db.ProductImages.Update(obj);
        }
    }
}
