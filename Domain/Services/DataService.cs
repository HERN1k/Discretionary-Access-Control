using DiscretionaryAccessControl.Domain.Enums;
using DiscretionaryAccessControl.Domain.Interfaces;
using DiscretionaryAccessControl.Domain.Objects;

namespace DiscretionaryAccessControl.Domain.Services
{
    public class DataService : IDataService
    {
        public void LogIn(string login, string password)
        {
            if (Program.Users.IsEmpty)
            {
                throw new ApplicationException("Critical error!");
            }

            ISubject user = Program.Users.Keys
                .Where(subject => subject.Login == login)
                .FirstOrDefault() ?? throw new ArgumentException("Incorrect login or password");

            if (password != user.Password)
            {
                throw new ArgumentException("Incorrect login or password");
            }

            Program.User = user;
        }

        public void LogOut()
        {
            if (Program.User != null)
            {
                Program.User = null;
            }
        }

        public List<string> ObjectsList()
        {
            if (Program.User == null)
            {
                throw new UnauthorizedAccessException();
            }

            List<string> objects = new();

            foreach (var obj in Program.Objects)
            {
                objects.Add(obj.ToString());
            }

            return objects;
        }

        public void AddObject(string name, string permission, string data)
        {
            if (Program.User == null)
            {
                throw new UnauthorizedAccessException();
            }

            if (Program.Objects.Any(obj => obj.Name == name))
            {
                throw new ArgumentException("An object with this name already exists");
            }

            if (!Enum.TryParse(typeof(ObjectPermission), permission, true, out var permissionObj))
            {
                throw new ApplicationException("Critical error!");
            }

            ObjectPermission objectPermission = (ObjectPermission)permissionObj;

            IObject obj = DataObject.Create(name, objectPermission, data)
                ?? throw new ApplicationException("Critical error!");

            Program.Objects.Add((DataObject)obj);
        }

        public string ReadObject(string name)
        {
            if (Program.User == null)
            {
                throw new UnauthorizedAccessException();
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Object name is empty");
            }

            DataObject obj = Program.Objects
                .Where(obj => obj.Name == name.Trim())
                .FirstOrDefault() ?? throw new ArgumentException($"The object with name of '{name}' was not found");

            return obj.Read();
        }

        public void EditObject(string name, string data)
        {
            if (Program.User == null)
            {
                throw new UnauthorizedAccessException();
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Object name is empty");
            }

            DataObject obj = Program.Objects
                .Where(obj => obj.Name == name.Trim())
                .FirstOrDefault() ?? throw new ArgumentException($"The object with name of '{name}' was not found");

            obj.Edit(data);
        }

        public void AddUser(string login, string password, string permission)
        {
            if (Program.User == null)
            {
                throw new UnauthorizedAccessException();
            }

            if (Program.Users.Any(user => user.Key.Login == login))
            {
                throw new ArgumentException("An user with this login already exists");
            }

            if (!Enum.TryParse(typeof(SubjectType), permission, true, out var permissionObj))
            {
                throw new ApplicationException("Critical error!");
            }

            SubjectType userPermission = (SubjectType)permissionObj;

            Subject user = (Subject)Subject.Create(login, password, userPermission);

            if (!Program.Users.TryAdd(user, byte.MinValue))
            {
                throw new ApplicationException("Critical error!");
            }
        }

        public List<string> UsersList()
        {
            if (Program.User == null)
            {
                throw new UnauthorizedAccessException();
            }

            List<string> users = new();

            foreach (var user in Program.Users)
            {
                users.Add(user.Key.ToString());
            }

            return users;
        }
    }
}