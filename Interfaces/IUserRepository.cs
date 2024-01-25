using testingStuff.models;

namespace testingStuff.Interfaces;

public interface IUserRepository
{
    public void AddUser(User user);
    public User? getUser(Guid id);
    public User? getUser(string username);
    public bool UserExists(Guid id);
    public bool UserExists(string username);
    public bool isAdmin(Guid user_id);
    public bool isReallyAdmin(Guid user_id, bool isAdminJwtValue);
    public bool hasPerm(Guid jwt_id, Guid user_id, bool isAdmin);
    public User setAdmin(Guid user_id, bool admin);
    public void userModified(User user);
    public void SaveChanges();
}