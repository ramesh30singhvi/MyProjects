using SHARP.Common.Constants;
using SHARP.DAL.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SHARP.DAL.Extensions
{
    /// <summary>
    /// Contains specific methods for mapping models to entities
    /// </summary>
    public static class CollectionUpdateExtensions
    {
        /// <summary>
        /// Returns an updated list that contains entities responsible for Many to Many relation.
        /// </summary>
        public static IEnumerable<TEntity> ActualizeForeignKeys<TEntity, TKey>(this IEnumerable<TEntity> source, IEnumerable<TKey> relatedIds, Func<TKey, TEntity> creator)
        {
            PropertyInfo prop = GetForeignKeyProperty(typeof(TEntity));

            var result = source.Where(i => relatedIds.Contains((TKey)prop.GetValue(i))).ToList();
            var toInsert = relatedIds.Where(i => !result.Select(x => (TKey)prop.GetValue(x)).Contains(i));

            result.AddRange(toInsert.Select(creator));

            return result;
        }

        private static PropertyInfo GetForeignKeyProperty(Type type)
        {
            return type.GetProperties().FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(ChildForeignKeyAttribute)));
        }

        /// <summary>
        /// Returns list of updated items and is merged with new items.
        /// </summary>
        public static IEnumerable<TEntity> Actualize<TEntity, TModel>(
            this IEnumerable<TEntity> source, IEnumerable<TModel> models, Func<TModel, TEntity> creator, Action<TModel, TEntity> mapper)
            where TEntity : IIdModel<int>
            where TModel : IIdModel<int>
        {
            var newModels = models.Where(i => i.Id.Equals(default));

            List<TEntity> newEntities = newModels.Select(creator).ToList();

            if (source != null)
            {
                foreach (var entityToUpdate in source)
                {
                    var model = models.FirstOrDefault(e => e.Id.Equals(entityToUpdate.Id));

                    if (model != null)
                    {
                        mapper(model, entityToUpdate);
                        newEntities.Add(entityToUpdate);
                    }
                }
            }

            return newEntities;
        }
    }
}
