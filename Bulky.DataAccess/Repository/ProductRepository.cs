using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using BulkyWeb.DataAcces.Data;

namespace Bulky.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Product product)
        {
            var objDb = _context.Products.FirstOrDefault(p => p.Id == product.Id);
            //manual mapping, this is why the update is not in the generic repository
            if (objDb != null)
            {
                objDb.Title = product.Title;
                objDb.ISBN = product.ISBN;
                objDb.Price = product.Price;
                objDb.Price50 = product.Price50;
                objDb.Price100 = product.Price100;
                objDb.ListPrice = product.ListPrice;
                objDb.Description = product.Description;
                objDb.CategoryId = product.CategoryId;
                objDb.Author = product.Author;
                if (product.ImageUrl != null)
                {
                    objDb.ImageUrl = product.ImageUrl;
                }
            }
        }
    }
}
