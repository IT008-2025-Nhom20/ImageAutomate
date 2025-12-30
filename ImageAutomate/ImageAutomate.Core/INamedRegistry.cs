namespace ImageAutomate.Core;

public interface INamedRegistry<T>
{
    void Register(string name, T item);
    bool Unregister(string name);
    T? GetOrCreate(string name);
    bool Contains(string name);
    IReadOnlyList<string> GetRegisteredNames();
}