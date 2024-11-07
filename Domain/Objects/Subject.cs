using System.Collections.Concurrent;

using DiscretionaryAccessControl.Domain.Enums;
using DiscretionaryAccessControl.Domain.Interfaces;

#pragma warning disable CS8618

namespace DiscretionaryAccessControl.Domain.Objects
{
    public class Subject : ISubject, IEquatable<Subject>
    {
        private Guid _id;

        public Guid Id
        {
            get
            {
                if (_id == Guid.Empty)
                {
                    _id = Guid.NewGuid();
                }

                return _id;
            }
        }

        private string _login;

        public string Login
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_login))
                {
                    throw new ArgumentException("Login cannot be null or empty", nameof(_login));
                }

                return _login;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Login cannot be null or empty", nameof(value));
                }

                _login = value;
            }
        }

        private string _password;

        public string Password
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_password))
                {
                    throw new ArgumentException("Password cannot be null or empty", nameof(_password));
                }

                return _password;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Password cannot be null or empty", nameof(value));
                }

                if (value.Length < 3)
                {
                    throw new ArgumentException("Password must be 3 or more characters", nameof(value));
                }

                _password = value;
            }
        }

        private SubjectType _permission;

        public SubjectType Permission
        {
            get
            {
                return _permission;
            }
            private set
            {
                if (value == SubjectType.None)
                {
                    throw new ArgumentException("Permission cannot be None", nameof(value));
                }

                _permission = value;
            }
        }

        private ConcurrentBag<Guid> _readObject = new();

        private ConcurrentBag<Guid> _writeObject = new();

        public ConcurrentBag<Guid> ReadObject { get => _readObject; }

        public ConcurrentBag<Guid> WriteObject { get => _writeObject; }

        private readonly string _separator = "\t";

        private Subject(string login, string password, SubjectType permission)
        {
            _id = Guid.NewGuid();
            Login = login;
            Password = password;
            Permission = permission;
        }

        public static ISubject Create(string login, string password, SubjectType permission)
        {
            if (Program.Users.IsEmpty && Program.User == null)
            {
                return new Subject(login, password, permission);
            }

            if (Program.User == null || !Program.CreatePermissions.Any(permission => permission == Program.User.Permission))
            {
                throw new ApplicationException($"Denied access");
            }

            if (permission == SubjectType.Root)
            {
                throw new ApplicationException($"Denied access");
            }

            if (Program.Users.Keys.Any(subject => subject.Login == login))
            {
                throw new ArgumentException($"User with this login already exists", nameof(login));
            }

            return new Subject(login, password, permission);
        }

        public DataObject AddObject(string name, string data)
        {
            if (Program.User == null)
            {
                throw new UnauthorizedAccessException();
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Object name is empty");
            }

            if (Program.Objects.Any(obj => obj.Name == name))
            {
                throw new ArgumentException("An object with this name already exists");
            }

            if (!Program.CreatePermissions.Any(permission => permission == Program.User.Permission))
            {
                throw new ApplicationException($"Denied access");
            }

            DataObject obj = (DataObject)DataObject.Create(name, data)
                ?? throw new ApplicationException("Critical error!");

            Program.Objects.Add(obj);

            return obj;
        }

        public DataObject GetObject(string name)
        {
            if (Program.User == null)
            {
                throw new UnauthorizedAccessException();
            }

            if (Program.User.Permission == SubjectType.None)
            {
                throw new ApplicationException($"Denied access");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Object name is empty");
            }

            DataObject obj = Program.Objects
                .Where(obj => obj.Name == name.Trim())
                .FirstOrDefault() ?? throw new ArgumentException($"The object with name of '{name}' was not found");

            if (Program.User.Permission == SubjectType.Root)
            {
                return obj;
            }

            if (!_readObject.Any(id => id == obj.Id))
            {
                throw new ApplicationException($"Denied access");
            }

            return obj;
        }

        public DataObject EditObject(string name, string data)
        {
            if (Program.User == null)
            {
                throw new UnauthorizedAccessException();
            }

            if (Program.User.Permission == SubjectType.None)
            {
                throw new ApplicationException($"Denied access");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Object name is empty");
            }

            DataObject obj = Program.Objects
                .Where(obj => obj.Name == name.Trim())
                .FirstOrDefault() ?? throw new ArgumentException($"The object with name of '{name}' was not found");

            if (Program.User.Permission == SubjectType.Root)
            {
                obj.Data = data;

                return obj;
            }

            if (!_writeObject.Any(id => id == obj.Id))
            {
                throw new ApplicationException($"Denied access");
            }

            obj.Data = data;

            return obj;
        }

        public static bool operator ==(Subject? left, Subject? right)
        {
            if (left is null)
                return right is null;

            return left.Equals(right);
        }

        public static bool operator !=(Subject? left, Subject? right)
        {
            return !(left == right);
        }

        public bool Equals(Subject? other)
        {
            if (other == null)
                return false;

            return Id == other.Id && Login == other.Login;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Subject person)
            {
                return Equals(person);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Login, Permission);
        }

        public override string ToString()
        {
            return $"{Login}{_separator}{Id}{_separator}{Permission}";
        }
    }
}