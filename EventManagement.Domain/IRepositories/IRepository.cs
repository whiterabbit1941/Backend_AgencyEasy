using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace EventManagement.Domain
{
    public interface IRepository<TEntity, TKey> where TEntity : class
    {

        IQueryable<TEntity> GetAllEntities(bool bIsAsTrackable = false);

        IQueryable<TEntity> GetFilteredEntities(bool bIsAsTrackable = false);

        #region SYNC METHODS

        TEntity GetEntityById(TKey id, bool bIsAsTrackable = false);

        dynamic GetPartialEntity(TKey id, string columns = "");

        dynamic GetPartialEntities(Expression<Func<TEntity, bool>> filterExpression = null, string columns = "");

        void CreateEntity(TEntity entity);

        void CreateBulkEntity(List<TEntity> entities);

        void UpdateEntity(TEntity entity);

        void UpdatePartialEntity(TEntity entity, JsonPatchDocument<TEntity> patchent);

        Task<int> UpdatePartialEntityAsync<TDto>(TEntity entity, JsonPatchDocument<TDto> patchent) where TDto : class;

        void DeleteEntityByID(TKey id);

        bool Exist(Expression<Func<TEntity, bool>> filterExpression);

        bool Any(Expression<Func<TEntity, bool>> filterExpression);

        int Count(Expression<Func<TEntity, bool>> filterExpression);

        bool SaveChanges();       

        #endregion

        #region ASYNC METHODS

        Task<List<TEntity>> GetAllEntitiesAsync();

        Task<TEntity> GetEntityByIdAsync(TKey id, bool bIsAsTrackable = false);

        Task<dynamic> GetPartialEntityAsync(TKey id, string columns = "");

        Task<int> UpdateEntityAsync(TKey key, Expression<Func<TEntity, TEntity>> updateExpression);

        Task<int> UpdateBulkEntityAsync(Expression<Func<TEntity, TEntity>> updateExpression, Expression<Func<TEntity, bool>> filterExpression = null);

        Task<int> UpdateBulkEntityForSerpAsync(Expression<Func<TEntity, TEntity>> updateExpression, Expression<Func<TEntity, bool>> filterExpression = null);
       
        Task<int> DeleteEntityAsync(TKey key);

        Task<int> DeleteBulkEntityAsync(Expression<Func<TEntity, bool>> filterExpression = null);

        Task<bool> ExistAsync(Expression<Func<TEntity, bool>> filterExpression);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filterExpression);

        Task<int> CountAsync(Expression<Func<TEntity, bool>> filterExpression);
     
        Task<bool> SaveChangesAsync();

        #endregion


    }
}
