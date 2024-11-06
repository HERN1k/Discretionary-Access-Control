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

        private Subject(string login, string password, SubjectType permission)
        {
            _id = Guid.NewGuid();
            Login = login;
            Password = password;
            Permission = permission;
        }

        public static ISubject Create(string login, string password, SubjectType permission)
        {
            if (Program.Users.IsEmpty)
            {
                return new Subject(login, password, permission);
            }

            if (Program.User == null || Program.User.Permission != SubjectType.Root)
            {
                throw new ApplicationException($"Denied access");
            }

            if (permission == SubjectType.Root)
            {
                throw new ArgumentException($"Denied access", nameof(permission));
            }

            if (Program.Users.Keys.Any(subject => subject.Login == login))
            {
                throw new ArgumentException($"User with this login already exists", nameof(login));
            }

            return new Subject(login, password, permission);
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
            return $"{Login}\t{Id}\t{Permission}";
        }
    }
}