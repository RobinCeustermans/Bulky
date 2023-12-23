using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class //a type that is a class
    {
        //T - Category
        IEnumerable<T> GetAll();

        T Get(Expression<Func<T,bool>> filter); //this way items can be retrieved based on multiple properties by usin a link expression

        void Add(T entity);

        //void Update(T entity);  //the reason the update is not used here, is because the update can be too specific for each model, same as save

        void Remove(T entity);

        void RemoveRange(IEnumerable<T> entities);
    }
}
