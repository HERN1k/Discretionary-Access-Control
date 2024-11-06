using System.Collections.Concurrent;

using DiscretionaryAccessControl.Domain.Enums;
using DiscretionaryAccessControl.Domain.Interfaces;
using DiscretionaryAccessControl.Domain.Objects;
using DiscretionaryAccessControl.Domain.Services;

namespace DiscretionaryAccessControl
{
    public static class Program
    {
        public static ISubject? User { get; set; } = null;

        public static readonly ConcurrentDictionary<Subject, byte> Users = new();

        public static readonly ConcurrentBag<DataObject> Objects = new();

        static Program()
        {
            Subject root = (Subject)Subject.Create("root", "root", SubjectType.Root);

            if (!Users.TryAdd(root, byte.MinValue))
            {
                throw new ApplicationException("Critical error!");
            }
#if DEBUG
            User = root;

            for (int i = 0; i < 10; i++)
            {
                Objects.Add((DataObject)DataObject.Create(
                    name: Guid.NewGuid().ToString(),
                    permission: ObjectPermission.RootOnly,
                    data: Guid.NewGuid().ToString()));
            }
            for (int i = 0; i < 10; i++)
            {
                Objects.Add((DataObject)DataObject.Create(
                    name: Guid.NewGuid().ToString(),
                    permission: ObjectPermission.Read,
                    data: Guid.NewGuid().ToString()));
            }
            for (int i = 0; i < 10; i++)
            {
                Objects.Add((DataObject)DataObject.Create(
                    name: Guid.NewGuid().ToString(),
                    permission: ObjectPermission.Write,
                    data: Guid.NewGuid().ToString()));
            }
#endif
        }

        public static void Main() => ConsoleService.Init();
    }
}