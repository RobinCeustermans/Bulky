using Bulky.Models;

namespace Bulky.DataAccess.Repository.IRepository
{
    public interface ICategoryRepository : IRepository<Category> //here you know which class you want to use, this base interface can thus be reused
    {
        //the oher methods come from IRepository
        void Update(Category category);
    }
}
