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

        private string _data;

        public string Data
        {
            get => _data;
            set => _data = value ?? string.Empty;
        }

        private readonly string _separator = "\t";

        private DataObject(string name, string? data = null)
        {
            _id = Guid.NewGuid();
            _data = data ?? string.Empty;
            Name = name.Trim();
        }

        public static IObject Create(string name, string? data = null)
        {
            if (Program.User == null)
            {
                throw new UnauthorizedAccessException();
            }

            return new DataObject(name, data);
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

            return Id == other.Id && Name == other.Name;
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
            return HashCode.Combine(Id, Name);
        }

        public override string ToString()
        {
            return $"{Name}{_separator}{Id}{_separator}{_data.Length}";
        }
    }
}