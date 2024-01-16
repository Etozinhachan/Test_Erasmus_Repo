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

    public bool isAdmin(Guid user_id)
    {
        return getUser(user_id).isAdmin;
    }

    public bool isReallyAdmin(Guid user_id, bool isAdminJwtValue)
    {
        var userHasAdmin = isAdmin(user_id);
        return !((!userHasAdmin && isAdminJwtValue) || (userHasAdmin && !isAdminJwtValue) || (!userHasAdmin && !isAdminJwtValue));
    }

    public bool UserExists(Guid id)
    {
        return _context.Users.Any(u => u.id == id);
    }

    public bool UserExists(string username)
    {
        return _context.Users.Any(u => u.UserName == username);
    }
}