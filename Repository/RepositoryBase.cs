using Azure.Core;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Models.Models;
using comercializadora_api.UnitofWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace comercializadora_api.Repository
{
    public class RepositoryBase<T> : ControllerBase, IRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected DbSet<T> dbSet;
        private readonly IUnitofWork _unitOfWork;

        public RepositoryBase(IUnitofWork unitOfwork)
        {
            _unitOfWork = unitOfwork;
            dbSet = _unitOfWork.Context.Set<T>();
        }

        public async Task<ActionResult<IEnumerable<T>>> GetAsync(Expression<Func<T, bool>> whereCondition = null,
                                         Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                                         string includeProperties = "")
        {
            IQueryable<T> query = _unitOfWork.Context.Set<T>();

            if (whereCondition != null)
            {
                query = query.Where(whereCondition);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }

        public async Task<Paginador<T>> GetFieldsFiltered(PaginadorRequestDto request)
        {
            Dictionary<string, string?> filters = request.filters;
            int page = request.page;
            int pageSize = request.pageSize;
            string? orderBy = request.orderBy;
            string? orderType = request.orderType;
            var query = _unitOfWork.Context.Set<T>().AsQueryable();

            // Filtros dinámicos
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    if (filter.Value != null)
                    {
                        var propertyInfo = typeof(T).GetProperty(filter.Key);
                        if (propertyInfo != null)
                        {
                            var parameter = Expression.Parameter(typeof(T), "e");
                            var property = Expression.Property(parameter, filter.Key);
                            var value = Expression.Constant(filter.Value);

                            // Compara según el tipo de propiedad
                            Expression condition = null;

                            if (propertyInfo.PropertyType == typeof(string))
                            {
                                // Si es un string, usamos Contains
                                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                                condition = Expression.Call(property, containsMethod, value);
                            }
                            else if (propertyInfo.PropertyType == typeof(int) || propertyInfo.PropertyType == typeof(long) ||
                                     propertyInfo.PropertyType == typeof(float) || propertyInfo.PropertyType == typeof(double))
                            {
                                // Si es un tipo numérico, usamos igualdad
                                condition = Expression.Equal(property, value);
                            }
                            else if (propertyInfo.PropertyType == typeof(bool))
                            {
                                var booleanValue = Convert.ToBoolean(filter.Value);
                                condition = Expression.Equal(property, Expression.Constant(booleanValue));
                            }
                            else if (propertyInfo.PropertyType == typeof(DateTime))
                            {
                                // Si es DateTime, comparas por igualdad
                                condition = Expression.Equal(property, value);
                            }
                            else
                            {
                                throw new InvalidOperationException($"Tipo de filtro no soportado: {propertyInfo.PropertyType}");
                            }

                            var lambda = Expression.Lambda<Func<T, bool>>(condition, parameter);
                            query = query.Where(lambda);
                        }
                    }
                }
            }

            // Ordenamiento dinámico
            if (!string.IsNullOrEmpty(orderBy))
            {
                var parameter = Expression.Parameter(typeof(T), "e");
                var property = Expression.Property(parameter, orderBy);
                var lambda = Expression.Lambda(property, parameter);

                // Agrega ordenamiento dinámico
                if (string.Equals(orderType, "desc", StringComparison.OrdinalIgnoreCase))
                {
                    var orderByDescendingMethod = typeof(Queryable)
                        .GetMethods()
                        .First(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2)
                        .MakeGenericMethod(typeof(T), property.Type);

                    query = (IQueryable<T>)orderByDescendingMethod.Invoke(null, new object[] { query, lambda });
                }
                else
                {
                    var orderByMethod = typeof(Queryable)
                        .GetMethods()
                        .First(m => m.Name == "OrderBy" && m.GetParameters().Length == 2)
                        .MakeGenericMethod(typeof(T), property.Type);

                    query = (IQueryable<T>)orderByMethod.Invoke(null, new object[] { query, lambda });
                }
            }

            // Total de elementos
            int totalItems = query.Count();

            // Paginación
                var result = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            

            // Total items por pagina
            int totalPaginas = (int)Math.Ceiling((double)totalItems / pageSize);

            var paginator = new Paginador<T>();
            paginator.items = result;
            paginator.totalItems = totalItems;
            paginator.paginaSiguiente = (page + 1) >= totalPaginas ? totalPaginas : (page + 1);
            paginator.itemsPorPagina = pageSize;
            paginator.totalPaginas = totalPaginas;
            paginator.paginaActual = page;
            paginator.ultimaPagina = totalPaginas;

            return paginator;
        }

        //Get Request
        public async Task<ActionResult<IEnumerable<T>>> Get()
        {
            var data = await dbSet.ToListAsync();
            return data;
        }

        //Create Request
        public async Task<ActionResult<T>> Create(T entity)
        {
            dbSet.Add(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }

        //Update Request
        public async Task<IActionResult> Update(int id, T entity)
        {
            //if (id != entity?.id)
            //{
            //    return BadRequest();
            //}

            var existingOrder = await dbSet.FindAsync(id);
            if (existingOrder == null)
            {
                return NotFound();
            }

            _unitOfWork.Context.Entry(existingOrder).CurrentValues.SetValues(entity);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return NoContent();
        }

        //Delete Request
        public async Task<IActionResult> Delete(int id)
        {
            var data = await dbSet.FindAsync(id);
            if (data == null)
            {
                return NotFound();
            }

            dbSet.Remove(data);
            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }
    }
}