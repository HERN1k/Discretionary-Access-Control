namespace DiscretionaryAccessControl.Domain.Interfaces
{
    public interface IDataService
    {
        void LogIn(string login, string password);
        void LogOut();
        List<string> ObjectsList();
        void AddObject(string name, string permission, string data);
        string ReadObject(string name);
        void EditObject(string name, string data);
        void AddUser(string login, string password, string permission);
        List<string> UsersList();
    }
}