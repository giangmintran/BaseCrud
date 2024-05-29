using BaseCrud.Dtos.Users;
using BaseCrud.Entites;
using BaseCrud.Interfaces;
using BaseCrud.Repositories;

namespace BaseCrud.Services
{
    public class UserService : IUserService
    {

        public UserService()
        {
        }

        public User Add(CreateUserDto input)
        {
            throw new NotImplementedException();
        }

        public User? Get(int id)
        {
            throw new NotImplementedException();
        }

        public void Update(User input)
        {
            throw new NotImplementedException();
        }
    }
}
