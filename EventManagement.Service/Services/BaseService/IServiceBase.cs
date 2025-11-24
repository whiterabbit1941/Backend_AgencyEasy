using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using EventManagement.Dto;

namespace EventManagement.Service
{
    public interface IService<TEntity, TKey> where TEntity : class
    {
        #region CRUD METHODS

        #region SYNC METHODS

        TEntity GetEntityById(TKey id);

        IEnumerable<TEntity> GetAllEntities(bool bIsAsTrackable = false);

        dynamic GetPartialEntity(TKey id, string columns = "");

        dynamic GetPartialEntities(Expression<Func<TEntity, bool>> filterExpression = null, string columns = "");

        void CreateEntity(TEntity entity);

        void CreateBulkEntity(List<TEntity> entities);

        void UpdateEntity(TEntity entity);

        void UpdatePartialEntityAsync(TEntity entity, JsonPatchDocument<TEntity> patchent);

        Task<int> UpdatePartialEntityAsync<TDto>(TEntity entity, JsonPatchDocument<TDto> patchent) where TDto : class;

        void DeleteEntityByID(TKey id);

        bool Exist(Expression<Func<TEntity, bool>> filterExpression);

        bool Any(Expression<Func<TEntity, bool>> filterExpression);

        int Count(Expression<Func<TEntity, bool>> filterExpression);

        bool SaveChanges();

        Task<PagedList<dynamic>> GetFilteredEntities(FilterOptionsModel filterOptionModel);

        #endregion

        #region ASYNC METHODS

        Task<List<TEntity>> GetAllEntitiesAsync();

        Task<TEntity> GetEntityByIdAsync(TKey id, bool bIsAsTrackable = false);

        Task<dynamic> GetPartialEntityAsync(TKey id, string columns = "");

        Task<TReturnDto> CreateEntityAsync<TReturnDto, TCreationDto>(TCreationDto dto);

        Task UpdateEntityAsync<TDto>(TKey key, TDto dto);

        Task<int> UpdateEntityAsync(TKey key, Expression<Func<TEntity, TEntity>> updateExpression);

        Task<int> UpdateBulkEntityAsync(Expression<Func<TEntity, TEntity>> updateExpression, Expression<Func<TEntity, bool>> filterExpression = null);

        Task<int> UpdateBulkEntityForSerpAsync(Expression<Func<TEntity, TEntity>> updateExpression, Expression<Func<TEntity, bool>> filterExpression = null);
       
        Task<int> DeleteEntityAsync(TKey id);

        Task<int> DeleteBulkEntityAsync(Expression<Func<TEntity, bool>> filterExpression = null);

        Task<bool> ExistAsync(Expression<Func<TEntity, bool>> filterExpression);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filterExpression);

        Task<int> CountAsync(Expression<Func<TEntity, bool>> filterExpression);

        Task<bool> SaveChangesAsync();

        #endregion

        bool ValidMappingExists(string fields);

        #endregion
    }
}
