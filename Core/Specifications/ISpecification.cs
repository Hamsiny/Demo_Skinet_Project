using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Core.Specifications
{
    public interface ISpecification<T>
    {
        //create specification interface
        Expression<Func<T, bool>> Criteria {get; }
        List<Expression<Func<T, object>>> Includes {get; }
        //create own order properties
        Expression<Func<T, object>> Orderby {get; }
        Expression<Func<T, object>> OrderbyDescending {get; }
        //create properties for pagination
        int Take {get;}
        int Skip {get;}
        bool IsPagingEnabled {get;}
    }
}