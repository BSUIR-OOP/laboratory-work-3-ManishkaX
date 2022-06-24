using System.Reflection;

namespace Serializer.Editing.Interfaces
{
    public interface IPropertyHandler
    {
        List<ExtendedPropertyInfo> GetProperties(object obj);

        bool TrySetProperty(object obj, string value, PropertyInfo prop);

        object CreateObject(string typeName);

        PropertyInfo GetProperty(object obj, string name);

        PropertyInfo GetProperty(object obj, int index);
    }
}
