using BaseCrud.Dtos.Users;
using BaseCrud.Entites;

namespace BaseCrud.Interfaces
{
    public interface IUserService
    {
        public User Add(CreateUserDto input);
        public User? Get(int id);
        public void Update(User input);
    }
}
