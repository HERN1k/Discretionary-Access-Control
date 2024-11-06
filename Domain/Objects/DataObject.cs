using DiscretionaryAccessControl.Domain.Enums;
using DiscretionaryAccessControl.Domain.Interfaces;

#pragma warning disable CS8618

namespace DiscretionaryAccessControl.Domain.Objects
{
    public class DataObject : IObject, IEquatable<DataObject>
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

        private string _name;

        public string Name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_name))
                {
                    throw new ArgumentException("Name cannot be null or empty", nameof(Name));
                }

                return _name;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Name cannot be null or empty", nameof(value));
                }

                _name = value;
            }
        }

        private ObjectPermission _permission;

        public ObjectPermission Permission { get; private set; }

        private string _data;

        private static SubjectType[] _createPermissions = { SubjectType.Root, SubjectType.User };

        private DataObject(string name, ObjectPermission permission, string? data = null)
        {
            _id = Guid.NewGuid();
            _permission = permission;
            _data = data ?? string.Empty;
            Name = name.Trim();
        }

        public static IObject Create(string name, ObjectPermission permission, string? data = null)
        {
            if (Program.User == null)
            {
                throw new UnauthorizedAccessException();
            }

            if (!_createPermissions.Contains(Program.User.Permission))
            {
                throw new ApplicationException($"Denied access");
            }

            return new DataObject(name, permission, data);
        }

        public void Edit(string input)
        {
            if (Program.User == null)
            {
                throw new UnauthorizedAccessException();
            }

            if (_permission == ObjectPermission.RootOnly)
            {
                if (Program.User.Permission != SubjectType.Root)
                {
                    throw new ApplicationException($"Denied access");
                }

                _data = input ?? string.Empty;
            }
            else if (_permission == ObjectPermission.Read)
            {
                if (Program.User.Permission == SubjectType.Root)
                {
                    _data = input ?? string.Empty;
                    return;
                }

                throw new ApplicationException($"Denied access");
            }
            else if (_permission == ObjectPermission.Write)
            {
                if (!_createPermissions.Contains(Program.User.Permission))
                {
                    throw new ApplicationException($"Denied access");
                }

                _data = input ?? string.Empty;
            }
            else
            {
                throw new ApplicationException("Critical error!");
            }
        }

        public string Read()
        {
            if (Program.User == null)
            {
                throw new UnauthorizedAccessException();
            }

            if (Program.User.Permission == SubjectType.None)
            {
                throw new ApplicationException($"Denied access");
            }

            return _data ?? string.Empty;
        }

        public object Clone()
        {
            if (Program.User == null)
            {
                throw new UnauthorizedAccessException();
            }

            if (!_createPermissions.Contains(Program.User.Permission))
            {
                throw new ApplicationException($"Denied access");
            }

            return new DataObject(Name, Permission, _data);
        }

        public static bool operator ==(DataObject? left, DataObject? right)
        {
            if (left is null)
                return right is null;

            return left.Equals(right);
        }

        public static bool operator !=(DataObject? left, DataObject? right)
        {
            return !(left == right);
        }

        public bool Equals(DataObject? other)
        {
            if (other == null)
                return false;

            return Id == other.Id && Name == other.Name && Permission == other.Permission;
        }

        public override bool Equals(object? obj)
        {
            if (obj is DataObject person)
            {
                return Equals(person);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, Permission);
        }

        public override string ToString()
        {
            return $"{Name}\t{Id}\t{_data.Length}";
        }
    }
}