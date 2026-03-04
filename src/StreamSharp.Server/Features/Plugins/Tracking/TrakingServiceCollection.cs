namespace StreamSharp.Server.Features.Plugins.Tracking;

internal class TrakingServiceCollection(IServiceCollection inner, List<ServiceDescriptor> tracking) : IServiceCollection
{
    public ServiceDescriptor this[int index]
    {
        get => inner[index];
        set => inner[index] = value;
    }

    public int Count => inner.Count;
    public bool IsReadOnly => inner.IsReadOnly;

    public void Add(ServiceDescriptor item)
    {
        tracking.Add(item);
        inner.Add(item);
    }

    public void Clear() => inner.Clear();
    public bool Contains(ServiceDescriptor item) => inner.Contains(item);
    public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => inner.CopyTo(array, arrayIndex);
    public IEnumerator<ServiceDescriptor> GetEnumerator() => inner.GetEnumerator();
    public int IndexOf(ServiceDescriptor item) => inner.IndexOf(item);
    public void Insert(int index, ServiceDescriptor item)
    {
        tracking.Add(item);
        inner.Insert(index, item);
    }
    public bool Remove(ServiceDescriptor item) => inner.Remove(item);
    public void RemoveAt(int index) => inner.RemoveAt(index);
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

