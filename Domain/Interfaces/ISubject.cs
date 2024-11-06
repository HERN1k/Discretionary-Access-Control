using DiscretionaryAccessControl.Domain.Enums;

namespace DiscretionaryAccessControl.Domain.Interfaces
{
    public interface ISubject
    {
        public Guid Id { get; }

        public string Login { get; }

        public string Password { get; }

        public SubjectType Permission { get; }

        public abstract static ISubject Create(string login, string password, SubjectType permission);
    }
}