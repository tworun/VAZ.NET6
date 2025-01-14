﻿using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VAZ.Domain.Common;

namespace VAZ.Infrastructure.Persistence.EntityFramework
{
	public class EfRepository<T> : IRepository<T> where T : BaseEntity
	{
		private readonly DbContext _context;
		private DbSet<T> _entities;
		public EfRepository(DbContext context)
		{
			_context = context;
		}
		protected virtual DbSet<T> Entities => _entities ?? _context.Set<T>();
		public IQueryable<T> GetAll => Entities;

		public IQueryable<T> GetAllNoTracking => Entities.AsNoTracking();

		public bool Any(Expression<Func<T, bool>> expression)
		{
			return Entities.Any(expression);
		}

		public int Commit()
		{
			try
			{
				return _context.SaveChanges();
			}
			catch (DbUpdateException exception)
			{
				GetFullErrorTextAndRollbackEntityChanges(exception);
				return -1;
			}
		}

		public int Delete(T entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

			Entities.Remove(entity);
			return Commit();
		}

		public int DeleteBulk(IEnumerable<T> entities)
		{
			if (entities == null)
				throw new ArgumentNullException(nameof(entities));

			Entities.RemoveRange(entities);
			return Commit();
		}

		public T Get(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes)
		{
			var query = Entities.Where(expression);
			return includes.Aggregate(query, (current, includeProperty) => current.Include(includeProperty)).FirstOrDefault();
		}

		public T GetById(int id)
		{
			return Entities.FirstOrDefault(x => x.Id == id);
		}

		public IQueryable<T> GetMany(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes)
		{
			var query = Entities.Where(expression);
			return includes.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
		}

		public T InsertWithoutCommit(T entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

			Entities.Add(entity);
			return entity;
		}

		public int InsertBulk(IEnumerable<T> entities)
		{
			if (entities == null)
				throw new ArgumentNullException(nameof(entities));

			Entities.AddRange(entities);
			return Commit();
		}

		public int Remove(T entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));
			try
			{
				Entities.Remove(entity);
				return 1;
			}
			catch
			{
				return -1;
			}
		}

		public int Update(T entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));
			try
			{
				Entities.Update(entity);
				Commit();
				return 1;
			}
			catch
			{
				return -1;
			}
		}

		public int UpdateWithoutCommit(T entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));
			try
			{
				Entities.Update(entity);
				return 1;
			}
			catch
			{
				return -1;
			}
		}

		protected string GetFullErrorTextAndRollbackEntityChanges(DbUpdateException exception)
		{
			if (_context is DbContext dbContext)
			{
				var entries = dbContext.ChangeTracker.Entries()
					.Where(e => e.State == EntityState.Added || e.State == EntityState.Modified).ToList();

				entries.ForEach(entry =>
				{
					try
					{
						entry.State = EntityState.Unchanged;
					}
					catch (InvalidOperationException)
					{
					}
				});
			}

			try
			{
				_context.SaveChanges();
				return exception.ToString();
			}
			catch (Exception ex)
			{
				return ex.ToString();
			}
		}

		public async Task<T> GetAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes)
		{
			var query = Entities.Where(expression);
			return await includes.Aggregate(query, (current, includeProperty) => current.Include(includeProperty)).FirstOrDefaultAsync();
		}

		public async Task<T> InsertAsync(T entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

			await Entities.AddAsync(entity);
			await CommitAsync();

			return entity;
		}

		public T Insert(T entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

			Entities.Add(entity);
			Commit();

			return entity;
		}

		public async Task<T> GetByIdAsync(int id)
		{
			return await Entities.FirstOrDefaultAsync(x => x.Id == id);
		}

		public async Task<int> CommitAsync()
		{
			try
			{
				return await _context.SaveChangesAsync();
			}
			catch (DbUpdateException exception)
			{
				GetFullErrorTextAndRollbackEntityChanges(exception);
				return -1;
			}
		}

		public async Task<int> UpdateAsync(T entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));
			try
			{
				Entities.Update(entity);
				await CommitAsync();
				return 1;
			}
			catch
			{
				return -1;
			}
		}

		public async Task<int> DeleteAsync(T entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

			await Task.Run(() => Entities.Remove(entity));

			return await CommitAsync();
		}
	}
}
