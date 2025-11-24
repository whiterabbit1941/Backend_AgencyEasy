using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using EventManagement.Domain;
using EventManagement.Dto;
using Microsoft.Extensions.Logging;

namespace EventManagement.Service
{
    public class ServiceBase<TEntity, TKey> : IService<TEntity, TKey> where TEntity : class
    {

        #region PRIVATE MEMBERS

        private readonly IRepository<TEntity, TKey> _repository;
        protected readonly ILogger<ServiceBase<TEntity, TKey>> _logger;

        #endregion


        #region CONSTRUCTOR

        public ServiceBase(IRepository<TEntity, TKey> repository, ILogger<ServiceBase<TEntity, TKey>> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        #endregion


        #region CRUD METHODS

        #region SYNC METHODS

        public TEntity GetEntityById(TKey id)
        {
            return _repository.GetEntityById(id);
        }

        public IEnumerable<TEntity> GetAllEntities(bool isTrackable = false)
        {
            return _repository.GetAllEntities(isTrackable);
        }

        public dynamic GetPartialEntity(TKey id, string columns = "")
        {
            return _repository.GetPartialEntity(id, columns);
        }

        public dynamic GetPartialEntities(Expression<Func<TEntity, bool>> filterExpression = null, string columns = "")
        {
            return _repository.GetPartialEntities(filterExpression, columns);
        }

        public void CreateEntity(TEntity entity)
        {
            _repository.CreateEntity(entity);
        }

        public void CreateBulkEntity(List<TEntity> entities)
        {
            _repository.CreateBulkEntity(entities);
        }

        public void UpdateEntity(TEntity entity)
        {
            _repository.UpdateEntity(entity);
        }

        public async Task<int> UpdatePartialEntityAsync<TDto>(TEntity entity, JsonPatchDocument<TDto> patchent) where TDto : class
        {
            return await _repository.UpdatePartialEntityAsync(entity, patchent);
        }

        public void UpdatePartialEntityAsync(TEntity entity, JsonPatchDocument<TEntity> patchent)
        {
            _repository.UpdatePartialEntity(entity, patchent);
        }

        public void DeleteEntityByID(TKey id)
        {
            _repository.DeleteEntityByID(id);
        }

        public bool Exist(Expression<Func<TEntity, bool>> filterExpression)
        {
            return _repository.Exist(filterExpression);
        }

        public bool Any(Expression<Func<TEntity, bool>> filterExpression)
        {
            return _repository.Any(filterExpression);
        }

        public int Count(Expression<Func<TEntity, bool>> filterExpression)
        {
            return _repository.Count(filterExpression);
        }

        public bool SaveChanges()
        {
            return _repository.SaveChanges();
        }

        #endregion

        #region ASYNC METHODS

        public async Task<List<TEntity>> GetAllEntitiesAsync()
        {
            return await _repository.GetAllEntitiesAsync();
        }

        public async Task<TEntity> GetEntityByIdAsync(TKey id, bool bIsAsTrackable = false)
        {
            return await _repository.GetEntityByIdAsync(id, bIsAsTrackable);
        }

        public async Task<dynamic> GetPartialEntityAsync(TKey id, string columns = "")
        {
            return await _repository.GetPartialEntityAsync(id, columns);
        }

        /// <summary>
        /// Creates an entity.
        /// </summary>
        /// <typeparam name="TReturnDto">Type to return</typeparam>
        /// <typeparam name="TCreationDto">TYpe for the Creation</typeparam>
        /// <param name="dto">Model to create an entity</param>
        /// <returns>Entity mapped Model</returns>
        public async Task<TReturnDto> CreateEntityAsync<TReturnDto, TCreationDto>(TCreationDto dto)
        {
            //Map the dto to Entity
            var entity = Mapper.Map<TEntity>(dto);

            //Add the entity to the dbContext.
            CreateEntity(entity);

            try
            {
                //save the changes to the db.
            if (!await SaveChangesAsync())
                {
                    //if saving failed then throw an exception.
                    throw new Exception($"Creating an entity failed on save.");
                }

            }
            catch (Exception ex)
            {
                var asdf = ex;
            }

            

            //Map and return the 
            return Mapper.Map<TReturnDto>(entity);
        }

        /// <summary>
        /// Fully Updates an entity.
        /// </summary>
        /// <typeparam name="TDto">Update Model Type</typeparam>
        /// <param name="key">Unique identifier for the entity</param>
        /// <param name="dto">Model to Update</param>
        /// <returns></returns>
        public async Task UpdateEntityAsync<TDto>(TKey key, TDto dto)
        {
            //Get an entity.
            var entity = GetEntityById(key);

            //Map the chnages from Dto to entity.
            Mapper.Map(dto, entity);

            //Add the Updated entity to the dbContext.
            UpdateEntity(entity);

            //Save changes to the db.
            if (!await SaveChangesAsync())
            {
                //if saving failed then throw an exception.
                throw new Exception($"Updating an entity {key} failed on save.");
            }
        }

        public async Task<int> UpdateEntityAsync(TKey key, Expression<Func<TEntity, TEntity>> updateExpression)
        {
            return await _repository.UpdateEntityAsync(key, updateExpression);
        }

        public async Task<int> UpdateBulkEntityAsync(Expression<Func<TEntity, TEntity>> updateExpression, Expression<Func<TEntity, bool>> filterExpression = null)
        {
            return await _repository.UpdateBulkEntityAsync(updateExpression, filterExpression);
        }

        public async Task<int> UpdateBulkEntityForSerpAsync(Expression<Func<TEntity, TEntity>> updateExpression, Expression<Func<TEntity, bool>> filterExpression = null)
        {
            return await _repository.UpdateBulkEntityForSerpAsync(updateExpression, filterExpression);
        }

        public async Task<int> DeleteEntityAsync(TKey id)
        {
            return await _repository.DeleteEntityAsync(id);
        }

        public async Task<int> DeleteBulkEntityAsync(Expression<Func<TEntity, bool>> filterExpression = null)
        {
            return await _repository.DeleteBulkEntityAsync(filterExpression);
        }

        public async Task<bool> ExistAsync(Expression<Func<TEntity, bool>> filterExpression)
        {
            return await _repository.ExistAsync(filterExpression);
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filterExpression)
        {
            return await _repository.AnyAsync(filterExpression);
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> filterExpression)
        {
            return await _repository.CountAsync(filterExpression);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _repository.SaveChangesAsync();
        }

        public async Task<PagedList<dynamic>> GetFilteredEntities(FilterOptionsModel filterOptionModel)
        {
            IQueryable<TEntity> filteredEntities = null;
            IQueryable<TEntity> sortedEntities = null;
            string selectClause = "";

            //Gets the Entity Queryable.
            IQueryable<TEntity> entityQueryable = _repository.GetFilteredEntities();
            try
            {
                //Apply filtering if applicable
                if (!String.IsNullOrEmpty(filterOptionModel.SearchQuery))
                {
                    //Apply the filtering expression to the customers Queryable.
                    filteredEntities = entityQueryable.Where(filterOptionModel.SearchQuery);
                }
                else
                {
                    filteredEntities = entityQueryable;
                }

                if (!String.IsNullOrEmpty(filterOptionModel.OrderBy))
                {
                    sortedEntities = ApplySort(filteredEntities, filterOptionModel.OrderBy, GetPropertyMapping());
                }
                else
                {
                    sortedEntities = ApplySort(filteredEntities, GetDefaultOrderByColumn(), GetPropertyMapping());
                }

                if (!string.IsNullOrEmpty(filterOptionModel.Fields))
                {
                    //Prepare the select clause.
                    selectClause = "new (" + filterOptionModel.Fields + " )";
                }
                else
                {
                    //Prepare the select clause.
                    selectClause = "new (" + GetDefaultFieldsToSelect() + " )";
                }

                var items = await sortedEntities.Skip((filterOptionModel.PageNumber - 1) * filterOptionModel.PageSize).Take(filterOptionModel.PageSize).Select(selectClause).ToDynamicListAsync();

                return new PagedList<dynamic>(items, sortedEntities.Count(), filterOptionModel.PageNumber, filterOptionModel.PageSize);

            }
            catch (Exception e)
            {
                var ex = e;
            }
            return new PagedList<dynamic>(new List<dynamic>(), sortedEntities.Count(), filterOptionModel.PageNumber, filterOptionModel.PageSize);
        }


        #endregion

        #endregion

        #region VIRTUAL METHODS


        public virtual Dictionary<string, PropertyMappingValue> GetPropertyMapping()
        {
            return new Dictionary<string, PropertyMappingValue>();
        }

        public virtual bool ValidMappingExists(string fields)
        {
            var propertyMapping = GetPropertyMapping();

            if (string.IsNullOrWhiteSpace(fields))
            {
                return true;
            }

            // the string is separated by ",", so we split it.
            var fieldsAfterSplit = fields.Split(',');

            // run through the fields clauses
            foreach (var field in fieldsAfterSplit)
            {
                // trim
                var trimmedField = field.Trim();

                // remove everything after the first " " - if the fields 
                // are coming from an orderBy string, this part must be 
                // ignored
                var indexOfFirstSpace = trimmedField.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ?
                    trimmedField : trimmedField.Remove(indexOfFirstSpace);

                // find the matching property
                if (!propertyMapping.ContainsKey(propertyName))
                {
                    return false;
                }
            }
            return true;
        }

        public virtual string GetDefaultOrderByColumn()
        {
            return "ID";
        }

        public virtual string GetDefaultFieldsToSelect()
        {
            return "ID";
        }

        #endregion

        #region PRIVATE METHODS

        public static IQueryable<T> ApplySort<T>(IQueryable<T> source, string orderBy,
           Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (mappingDictionary == null)
            {
                throw new ArgumentNullException("mappingDictionary");
            }

            if (string.IsNullOrWhiteSpace(orderBy))
            {
                return source;
            }
            // the orderBy string is separated by ",", so we split it.
            var orderByAfterSplit = orderBy.Split(',');

            // apply each orderby clause in reverse order - otherwise, the 
            // IQueryable will be ordered in the wrong order
            foreach (var orderByClause in orderByAfterSplit.Reverse())
            {
                // trim the orderByClause, as it might contain leading 
                // or trailing spaces. Can't trim the var in foreach,
                // so use another var.
                var trimmedOrderByClause = orderByClause.Trim();

                // if the sort option ends with with " desc", we order
                // descending, otherwise ascending
                var orderDescending = trimmedOrderByClause.EndsWith(" desc");

                // remove " asc" or " desc" from the orderByClause, so we 
                // get the property name to look for in the mapping dictionary
                var indexOfFirstSpace = trimmedOrderByClause.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ?
                    trimmedOrderByClause : trimmedOrderByClause.Remove(indexOfFirstSpace);

                // find the matching property
                if (!mappingDictionary.ContainsKey(propertyName))
                {
                    throw new ArgumentException($"Key mapping for {propertyName} is missing");
                }

                // get the PropertyMappingValue
                var propertyMappingValue = mappingDictionary[propertyName];

                if (propertyMappingValue == null)
                {
                    throw new ArgumentNullException("propertyMappingValue");
                }

                // Run through the property names in reverse
                // so the orderby clauses are applied in the correct order
                foreach (var destinationProperty in propertyMappingValue.DestinationProperties.Reverse())
                {
                    // revert sort order if necessary
                    if (propertyMappingValue.Revert)
                    {
                        orderDescending = !orderDescending;
                    }
                    source = source.OrderBy(destinationProperty + (orderDescending ? " descending" : " ascending"));
                }
            }
            return source;
        }

        #endregion

    }
}
