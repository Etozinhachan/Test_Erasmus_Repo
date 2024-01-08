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

    public User? getUser(Guid id)
    {
        return _context.Users.Where(u => u.id == id).FirstOrDefault();
    }

    public User? getUser(string username)
    {
        return _context.Users.Where(u => u.UserName == username).FirstOrDefault();
    }

    public bool UserExists(Guid id)
    {
        throw new NotImplementedException();
    }

    public bool UserExists(string username)
    {
        throw new NotImplementedException();
    }
}