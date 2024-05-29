using BaseCrud.Common;
using BaseCrud.Database;
using BaseCrud.Entites;

namespace BaseCrud.Repositories
{
    public class UserRepository : BaseCrudEntity<User>
    {
        public UserRepository(ApplicationDbContext db) : base(db)
        {
        }

        public override void Dispose()
        { 
        }
    }
}
