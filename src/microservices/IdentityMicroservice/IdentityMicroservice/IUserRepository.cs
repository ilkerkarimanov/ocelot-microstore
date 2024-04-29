namespace IdentityMicroservice
{
    public interface IUserRepository
    {
        User? GetUser(string email);
        void InsertUser(User user);
    }
}
