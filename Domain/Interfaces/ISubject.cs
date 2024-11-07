using System.Collections.Concurrent;

using DiscretionaryAccessControl.Domain.Enums;
using DiscretionaryAccessControl.Domain.Objects;

namespace DiscretionaryAccessControl.Domain.Interfaces
{
    public interface ISubject
    {
        Guid Id { get; }

        string Login { get; }

        string Password { get; }

        SubjectType Permission { get; }

        ConcurrentBag<Guid> ReadObject { get; }

        ConcurrentBag<Guid> WriteObject { get; }

        abstract static ISubject Create(string login, string password, SubjectType permission);

        DataObject AddObject(string name, string data);

        DataObject GetObject(string name);

        DataObject EditObject(string name, string data);
    }
}