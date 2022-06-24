using System.Reflection;
using Serializer.Editing.Interfaces;

namespace Serializer.Editing
{
    public class PropertyHandler: IPropertyHandler
    {
        public List<ExtendedPropertyInfo> GetProperties(object obj) =>
            obj.GetType().GetProperties().Select(prop => new ExtendedPropertyInfo(prop, prop.GetValue(obj))).ToList();
        

        public bool TrySetProperty(object obj, string value, PropertyInfo prop)
        {
            try
            {
                prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType), null);
                return true;
            }
            catch(FormatException e)
            {
                return false;
            }
        }

        public object CreateObject(string typeName)
        {
           var type = Type.GetType(typeName);
           if (type == null) 
               return null;
           
           var info = type.GetConstructor(Type.EmptyTypes);
           
           return (info != null ? info.Invoke(Array.Empty<object>()) : null)!;
        }

        public PropertyInfo GetProperty(object obj, string name) =>
            obj.GetType().GetProperty(name)!;
        

        public PropertyInfo GetProperty(object obj, int index)
        {
            return GetProperties(obj)[index].Property;
        }
    }
}
