using BulkyBook.DataAccess.Repository.Generic;
using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository.Product
{
    public interface IProductImagesRepository : IGenericRepository<ProductImageModel>
    {
        void Update(ProductImageModel obj);
    }
}
