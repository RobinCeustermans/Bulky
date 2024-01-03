using System.Linq.Expressions;

namespace Bulky.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class //a type that is a class
    {
        //T - Category
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);

        //this way with filters, items can be retrieved based on multiple properties by using a link expression
        T Get(Expression<Func<T,bool>> filter, string? includeProperties = null, bool tracked = false); //by setting on false, EF will no longer track and you have to update manually 
        void Add(T entity);

        //void Update(T entity);  //the reason the update is not used here, is because the update can be too specific for each model, same as save

        void Remove(T entity);

        void RemoveRange(IEnumerable<T> entities);
    }
}
