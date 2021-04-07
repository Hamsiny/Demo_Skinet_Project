using System.Linq;
using Core.Entities;
using Core.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class SpecificationEvaluator<TEntity> where TEntity : BaseEntity
    {
        //create evaluator of specification
        //return iqueryable of products
        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, 
        ISpecification<TEntity> spec)
        {
            var query = inputQuery;

            if (spec.Criteria != null)
            {
                query = query.Where(spec.Criteria); //p => p.ProductTypeId == id
            }

            //put order properties in specification into evaluator
            if (spec.Orderby != null)
            {
                query = query.OrderBy(spec.Orderby);
            }

            if (spec.OrderbyDescending != null)
            {
                query = query.OrderByDescending(spec.OrderbyDescending);
            }

            //put paging condition after order and filter
            if (spec.IsPagingEnabled)
            {
                query = query.Skip(spec.Skip).Take(spec.Take);
            }

            // doing same with include before in ProductRepository class
            query = spec.Includes.Aggregate(query, 
                (current, include) => current.Include(include));

            return query;
        }
    }
}