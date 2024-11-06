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

        public static readonly List<string> AuthorizationsLog = new();

        public static readonly List<string> EventsLog = new();

        public static readonly List<string> ExceptionsLog = new();

        static Program()
        {
            Subject root = (Subject)Subject.Create("root", "root", SubjectType.Root);

            if (!Users.TryAdd(root, byte.MinValue))
            {
                throw new ApplicationException("Critical error!");
            }
#if DEBUG
            User = root;

            for (int i = 0; i < 30; i++)
            {
                if (i < 10)
                {
                    Objects.Add((DataObject)DataObject.Create(
                        name: Guid.NewGuid().ToString().ToUpper()[24..],
                        permission: ObjectPermission.RootOnly,
                        data: "Some data!"));
                }
                else if (i >= 10 && i < 20)
                {
                    Objects.Add((DataObject)DataObject.Create(
                        name: Guid.NewGuid().ToString().ToUpper()[24..],
                        permission: ObjectPermission.Read,
                        data: "Some data!"));
                }
                else
                {
                    Objects.Add((DataObject)DataObject.Create(
                        name: Guid.NewGuid().ToString().ToUpper()[24..],
                        permission: ObjectPermission.Write,
                        data: "Some data!"));
                }
            }
#endif
        }

        public static void Main()
        {
            int code = 1;
            while (code == 1)
            {
                try
                {
                    ConsoleService.Init(ref code);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                    Console.ReadLine();
                }
            }
        }
    }
}