using System.Globalization;

using DiscretionaryAccessControl.Domain.Enums;
using DiscretionaryAccessControl.Domain.Interfaces;
using DiscretionaryAccessControl.Domain.Objects;

namespace DiscretionaryAccessControl.Domain.Services
{
    public class DataService : IDataService
    {
        public object LockObject { get; init; } = new();

        private readonly string _separator = "\t";

        private readonly CultureInfo _culture = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US");

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
            WriteAuthorizationLog();
        }

        public void LogOut()
        {
            if (Program.User != null)
            {
                WriteAuthorizationLog(Program.User);
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
            WriteEventLog($"Obj id: {obj.Id}", $"User: {Program.User.Login}, Obj name: {obj.Name}", AppEvent.Object_Added);
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

            string data = obj.Read();
            WriteEventLog($"Obj id: {obj.Id}", $"User: {Program.User.Login}, Obj name: {obj.Name}", AppEvent.Object_Read);

            return data;
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
            WriteEventLog($"Obj id: {obj.Id}", $"User: {Program.User.Login}, Obj name: {obj.Name}", AppEvent.Object_Edit);
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

            WriteEventLog($"User id: {Program.User.Id}", $"User: {Program.User.Login}, New user: {user.Login}", AppEvent.User_Added);
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

        public void WriteExceptionLog(Exception ex)
        {
            if (Program.User == null || ex == null)
            {
                return;
            }

            string log = string.Concat(new string[7]
            {
                ex.Message,
                _separator,
                Program.User.Id.ToString(),
                _separator,
                Program.User.Permission.ToString(),
                _separator,
                DateTime.Now.ToString("d MMMM yyyy H:m:s", _culture)
            });

            lock (LockObject)
            {
                Program.ExceptionsLog.Add(log);
            }
        }

        private void WriteAuthorizationLog()
        {
            if (Program.User == null)
            {
                return;
            }

            string log = string.Concat(new string[9]
            {
                "Authorization",
                _separator,
                Program.User.Id.ToString(),
                _separator,
                Program.User.Login.ToString(),
                _separator,
                Program.User.Permission.ToString(),
                _separator,
                DateTime.Now.ToString("d MMMM yyyy H:m:s", _culture)
            });

            lock (LockObject)
            {
                Program.AuthorizationsLog.Add(log);
            }
        }

        private void WriteAuthorizationLog(ISubject user)
        {
            if (user == null)
            {
                return;
            }

            string log = string.Concat(new string[9]
            {
                "Deauthorization",
                _separator,
                user.Id.ToString(),
                _separator,
                user.Login.ToString(),
                _separator,
                user.Permission.ToString(),
                _separator,
                DateTime.Now.ToString("d MMMM yyyy H:m:s", _culture)
            });

            lock (LockObject)
            {
                Program.AuthorizationsLog.Add(log);
            }
        }

        private void WriteEventLog(string id, string data, AppEvent appEvent)
        {
            if (Program.User == null)
            {
                return;
            }

            string log = string.Concat(new string[7]
            {
                appEvent.ToString().Replace('_', ' '),
                _separator,
                id,
                _separator,
                data,
                _separator,
                DateTime.Now.ToString("d MMMM yyyy H:m:s", _culture)
            });

            lock (LockObject)
            {
                Program.EventsLog.Add(log);
            }
        }
    }
}