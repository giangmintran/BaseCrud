using BaseCrud.Database;
using Microsoft.EntityFrameworkCore;

namespace BaseCrud.Common
{
    public abstract class BaseCrudEntity<T>(ApplicationDbContext db) : IDisposable where T : class
    {

        private readonly ApplicationDbContext db = db;    //my database context
        private DbSet<T> entities = db.Set<T>();  //specific set

        public T Add(T entity)
        {
            entities.Add(entity);
            db.SaveChanges();
            return entity;
        }

        public T? Get(int id)
        {
            return entities.Find(id);
        }

        public List<T> GetAll()
        {
            return [.. entities];
        }

        public void Delete(int id)
        {
            T? objectToDelete = entities.Find(id);
            if (objectToDelete != null)
            {
                entities.Remove(objectToDelete);
                db.SaveChanges();
            }
        }

        public void Delete(T entity)
        {
            entities.Remove(entity);
            db.SaveChanges();
        }

        public void Delete(List<T> items)
        {
            foreach (T entity in items)
            {
                entities.Remove(entity);
            }
            db.SaveChanges();
        }

        public void Update(T entity)
        {
            entities.Attach(entity);
            db.Entry(entity).State = EntityState.Modified;
            db.SaveChanges();
        }

        public abstract void Dispose();
    }
}
