using DiscretionaryAccessControl.Domain.Enums;

namespace DiscretionaryAccessControl.Domain.Interfaces
{
    public interface IObject : ICloneable
    {
        public Guid Id { get; }
        public string Name { get; set; }
        public ObjectPermission Permission { get; }
        public abstract static IObject Create(string name, ObjectPermission permission, string? data = null);
        void Edit(string input);
        string Read();
    }
}