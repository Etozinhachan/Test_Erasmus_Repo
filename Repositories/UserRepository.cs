using testingStuff.data;
using testingStuff.Interfaces;
using testingStuff.models;

namespace testingStuff.Repositories;

public class UserRepository : IUserRepository
{

    private readonly DbDataContext _context;

    public UserRepository(DbDataContext context)
    {
        _context = context;
    }

    public void AddUser(User user){
        _context.Users.Add(user);
        _context.SaveChanges();
    }
}