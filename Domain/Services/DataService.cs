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

            if (Program.User != null)
            {
                WriteAuthorizationLog(Program.User);
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

        public void AddObject(string name, string data)
        {
            if (Program.User == null)
            {
                throw new UnauthorizedAccessException();
            }

            DataObject obj = Program.User.AddObject(name, data);

            WriteEventLog(
                $"Obj id: {obj.Id}",
                $"User: {Program.User.Login}, Obj name: {obj.Name}",
                AppEvent.Object_Added);
        }

        public string ReadObject(string name)
        {
            if (Program.User == null)
            {
                throw new UnauthorizedAccessException();
            }

            DataObject obj = Program.User.GetObject(name);

            WriteEventLog(
                $"Obj id: {obj.Id}",
                $"User: {Program.User?.Login ?? "null"}, Obj name: {obj.Name}",
                AppEvent.Object_Read);

            return obj.Data;
        }

        public void EditObject(string name, string data)
        {
            if (Program.User == null)
            {
                throw new UnauthorizedAccessException();
            }

            DataObject obj = Program.User.EditObject(name, data);

            WriteEventLog(
                $"Obj id: {obj.Id}",
                $"User: {Program.User.Login}, Obj name: {obj.Name}",
                AppEvent.Object_Edit);
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

        public void AddPermission(string login, string name, string permission)
        {
            if (Program.User == null)
            {
                throw new UnauthorizedAccessException();
            }

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Object name or user login is empty");
            }

            if (!Program.CreatePermissions.Any(permission => permission == Program.User.Permission))
            {
                throw new ApplicationException($"Denied access");
            }

            ISubject user = Program.Users
                .Where(user => user.Key.Login == login)
                .FirstOrDefault().Key ?? throw new ArgumentException($"The user with login of '{name}' was not found");

            IObject obj = Program.Objects
                .Where(obj => obj.Name == name)
                .FirstOrDefault() ?? throw new ArgumentException($"The object with name of '{name}' was not found");

            if (user.Permission == SubjectType.Root && Program.User.Permission != SubjectType.Root)
            {
                throw new ApplicationException($"Denied access");
            }

            if (permission == "Read")
            {
                if (Program.User.Permission == SubjectType.Root)
                {
                    user.ReadObject.Add(obj.Id);
                }
                else if (Program.User.Permission == SubjectType.User)
                {
                    if (user.Permission != SubjectType.Observer)
                    {
                        throw new ApplicationException($"Denied access");
                    }

                    user.ReadObject.Add(obj.Id);
                }
                else
                {
                    throw new ApplicationException($"Denied access");
                }
            }
            else if (permission == "Write")
            {
                if (Program.User.Permission == SubjectType.Root)
                {
                    user.WriteObject.Add(obj.Id);
                }
                else if (Program.User.Permission == SubjectType.User)
                {
                    if (user.Permission != SubjectType.Observer)
                    {
                        throw new ApplicationException($"Denied access");
                    }
                }
                else
                {
                    throw new ApplicationException($"Denied access");
                }
            }
            else if (permission == "Read and Write")
            {
                if (Program.User.Permission == SubjectType.Root)
                {
                    user.ReadObject.Add(obj.Id);
                    user.WriteObject.Add(obj.Id);
                }
                else if (Program.User.Permission == SubjectType.User)
                {
                    if (user.Permission != SubjectType.Observer)
                    {
                        throw new ApplicationException($"Denied access");
                    }

                    user.ReadObject.Add(obj.Id);
                }
                else
                {
                    throw new ApplicationException($"Denied access");
                }
            }
            else
            {
                throw new ApplicationException($"Denied access");
            }

            WriteEventLog(
                $"User ID: {Program.User.Id}",
                $"User {Program.User.Login} added permission '{permission}' for object '{obj.Name}' for user {user.Login}",
                AppEvent.Added_Permission);
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