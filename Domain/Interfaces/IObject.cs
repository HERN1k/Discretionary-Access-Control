namespace DiscretionaryAccessControl.Domain.Interfaces
{
    public interface IObject
    {
        Guid Id { get; }

        string Name { get; set; }

        string Data { get; set; }

        abstract static IObject Create(string name, string? data = null);
    }
}