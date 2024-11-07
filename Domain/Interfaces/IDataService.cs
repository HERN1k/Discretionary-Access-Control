namespace DiscretionaryAccessControl.Domain.Interfaces
{
    public interface IDataService
    {
        object LockObject { get; }

        void LogIn(string login, string password);

        void LogOut();

        List<string> ObjectsList();

        void AddObject(string name, string data);

        string ReadObject(string name);

        void EditObject(string name, string data);

        void AddUser(string login, string password, string permission);

        void AddPermission(string login, string name, string permission);

        List<string> UsersList();

        void WriteExceptionLog(Exception ex);
    }
}