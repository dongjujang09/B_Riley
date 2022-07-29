using B_Riley.BankingApp.Models.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace B_Riley.BankingApp.Data.Repositories
{
    public class BaseRepository<T> where T : BaseEntity
    {
        public BaseRepository(BankingAppContext context)
        {
            this.Context = context;
        }

        protected BankingAppContext Context { get; set; }


        public IQueryable<T> GetQueryable() => Context.Set<T>().AsQueryable();
        public void SetAsUpdate(T entity) => SetEntityState(entity, EntityState.Modified);
        public void SetValues(T source, T target) => Context.Entry<T>(source).CurrentValues.SetValues(target);
        public void SetEntityState(T entity, EntityState state) => Context.Entry<T>(entity).State = state;


        public void SaveChanges() => Context.SaveChanges();
        public async Task SaveChangesAsync() => await Context.SaveChangesAsync();


        public virtual async Task<IEnumerable<T>> GetAllAsync() => await GetQueryable().ToListAsync();
        public virtual async Task<T?> FindAsync(int id)
        {
            if (id < 0) throw new ArgumentOutOfRangeException(nameof(id));
            
            return await Context.Set<T>().FindAsync(id);
        }
        public virtual async Task<T?> InsertOrUpdateAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            if (entity.Id == 0)
            {
                await Context.Set<T>().AddAsync(entity);
            }
            else
            {
                SetAsUpdate(entity);
            }
            await SaveChangesAsync();
            return entity;
        }

        public virtual void Delete(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (entity.Id < 0) throw new ArgumentOutOfRangeException(nameof(entity.Id));

            Context.Remove(entity);
            SaveChangesAsync();
        }
    }
}
