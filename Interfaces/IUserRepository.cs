using testingStuff.models;

namespace testingStuff.Interfaces;

public interface IUserRepository
{
    public void AddUser(User user);
    public User getUser(Guid id);
    public User getUser(string username);
    public bool UserExists(Guid id);
    public bool UserExists(string username);
}