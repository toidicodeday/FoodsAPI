using SampleWebApiAspNetCore.Entities;
using SampleWebApiAspNetCore.Helpers;
using SampleWebApiAspNetCore.Models;
using System.Linq.Dynamic.Core;

namespace SampleWebApiAspNetCore.Repositories
{
    public class FoodSqlRepository : IFoodRepository
    {
        private readonly MyDbContext _foodDbContext;

        public FoodSqlRepository(MyDbContext foodDbContext)
        {
            _foodDbContext = foodDbContext;
        }

        public FoodEntity GetSingle(int id)
        {
            return _foodDbContext.Foods.FirstOrDefault(x => x.Id == id);
        }

        public void Add(FoodEntity item)
        {
            _foodDbContext.Foods.Add(item);
        }

        public void Delete(int id)
        {
            FoodEntity foodItem = GetSingle(id);
            _foodDbContext.Foods.Remove(foodItem);
        }

        public FoodEntity Update(int id, FoodEntity item)
        {
            _foodDbContext.Foods.Update(item);
            return item;
        }

        public IQueryable<FoodEntity> GetAll(QueryParameters queryParameters)
        {
            IQueryable<FoodEntity> _allItems = _foodDbContext.Foods.OrderBy(queryParameters.OrderBy,
              queryParameters.IsDescending());

            if (queryParameters.HasQuery())
            {
                _allItems = _allItems
                    .Where(x => x.Calories.ToString().Contains(queryParameters.Query.ToLowerInvariant())
                    || x.Name.ToLowerInvariant().Contains(queryParameters.Query.ToLowerInvariant()));
            }

            return _allItems
                .Skip(queryParameters.PageCount * (queryParameters.Page - 1))
                .Take(queryParameters.PageCount);
        }

        public int Count()
        {
            return _foodDbContext.Foods.Count();
        }

        public bool Save()
        {
            return (_foodDbContext.SaveChanges() >= 0);
        }

        public ICollection<FoodEntity> GetRandomMeal()
        {
            List<FoodEntity> toReturn = new List<FoodEntity>();

            toReturn.Add(GetRandomItem("Starter"));
            toReturn.Add(GetRandomItem("Main"));
            toReturn.Add(GetRandomItem("Dessert"));

            return toReturn;
        }

        private FoodEntity GetRandomItem(string type)
        {
            return _foodDbContext.Foods
                .Where(x => x.Type == type)
                .OrderBy(o => Guid.NewGuid())
                .FirstOrDefault();
        }
    }
}
