using System.Collections;
using Serializer.Editing.Interfaces;
using Serializer.Models;
using Serializer.Serialization.Interfaces;

namespace Serializer.Editing
{
    public class ConsoleEditor
    {
        private class ActionInfo
        {
            public string Description { get; }

            public Action Action { get; }


            public ActionInfo(Action action, string description)
            {
                Action = action;
                Description = description;
            }
        }


        private bool _isActive;

        private readonly IPropertyHandler _propertyHandler;

        private readonly ISerializer _serializer;

        private readonly IFilesHandler _filesHandler;

        private readonly List<object> _editingObjects;

        private Dictionary<int, ActionInfo> _actions;

        private const string ObjectStr = "OBJECT";

        private const string EnumerableString = "ENUMERABLE";

        private const string PrimitiveStr = "PRIMITIVE";

        private const string ListStr = "LIST";


        public ConsoleEditor(IPropertyHandler propertyHandler, ISerializer serializer, IFilesHandler filesHandler, object editingObject)
        {
            _isActive = false;
            _propertyHandler = propertyHandler;
            _serializer = serializer;
            _filesHandler = filesHandler;

            _editingObjects = new List<object> { editingObject };
            InitActions();
        }


        public void Start()
        {
            _isActive = true;

            while (_isActive)
            {
                Console.Clear();
                InvokeAction();

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }

        private void PrintEditingObject() =>
            PrintObject(_editingObjects[^1]);
        

        private void PrintObject(object obj)
        {
            if (obj is IList list)
            {
                PrintObjectCollection(list);
                return;
            }

            if (obj is IEnumerable || obj.GetType().IsPrimitive)
                throw new Exception("Invalid type");
            
            PrintObjectProperties(obj);
        }

        private static void PrintObjectCollection(ICollection list)
        {
            if (list.Count > 0)
            {
                Console.WriteLine("Editable collection: ");
                var index = 1;
                foreach (var item in list)
                {
                    if (item is IEnumerable)
                    {
                        Console.WriteLine("  {0}. [{1}] - {2}", index, item.GetType().ToString(),
                            item is IList ? ListStr : EnumerableString);
                    }
                    else if (item.GetType().IsPrimitive)
                        Console.WriteLine("  {0}. [{1}] - {2}", index, item.GetType().ToString(), PrimitiveStr);
                    else
                        Console.WriteLine("  {0}. [{1}] - {2}", index, item.GetType().ToString(), ObjectStr);

                    ++index;
                }
            }
            else
                Console.WriteLine("This collection is empty");
            
            Console.WriteLine();
        }

        private void PrintObjectProperties(object obj)
        {
            var list = _propertyHandler.GetProperties(obj);

            if (list.Count > 0)
            {
                Console.WriteLine("Object properties: ");
                var index = 1;
                foreach (var item in list)
                {
                    if (item.Property.PropertyType.IsPrimitive)
                        Console.WriteLine("  {0}. [{1}] - \"{2}\" = {3}", index, item.Property.PropertyType.ToString(), item.Property.Name, item.PropertyValue);
                    else
                        Console.WriteLine("  {0}. [{1}] - \"{2}\" - {3}", index, item.Property.PropertyType.ToString(), item.Property.Name, ObjectStr);

                    ++index;
                }
            }
            else
                Console.WriteLine("Object has no properties to edit");
            Console.WriteLine();
        }

        private static int EnterCollectionIndex(ICollection list)
        {
            while (true)
            {
                Console.Write("Enter index: ");
                if ((int.TryParse(Console.ReadLine(), out int res)) && (res > 0) && (res <= list.Count))
                    return res - 1;
                else
                    Console.WriteLine("Incorrect index");
            }
        }

        private void MoveIn()
        {
            var obj = _editingObjects[^1];
            var type = obj.GetType();

            PrintObject(obj);

            if (obj is IList)
            {
                var index = EnterCollectionIndex((obj as IList)!);
                _editingObjects.Add((obj as IList)[index]);
            }
            else if ((obj is not IEnumerable) && (!type.IsPrimitive))
            {
                var props = _propertyHandler.GetProperties(obj);
                var index = EnterCollectionIndex(props);
                var propInfo = props[index];

                if (!propInfo.Property.PropertyType.IsPrimitive)
                    _editingObjects.Add(propInfo.PropertyValue);
                else
                    Console.WriteLine("Cannot \"unwrap\" this type: {0}", propInfo.Property.PropertyType.ToString());
            }
            else
                Console.WriteLine("Cannot \"unwrap\" this type: {0}", type.ToString());
        }

        private void MoveOut()
        {
            if (_editingObjects.Count > 1)
                _editingObjects.RemoveAt(_editingObjects.Count - 1);
            else
                Console.WriteLine("Cannot remove outermost object");
        }

        private void Exit() => _isActive = false;

        private void InvokeAction()
        {
            PrintEditingObject();
            Console.WriteLine();

            Console.WriteLine("Choose an action:");
            foreach (var (key, value) in _actions)
                Console.WriteLine("  Press {0} to {1}", key, value.Description);
            Console.WriteLine();

            while (true)
            {
                Console.Write("Enter action key: ");
                var keyStr = Console.ReadLine();

                if (!int.TryParse(keyStr, out var key))
                    Console.WriteLine("Invalid input");
                else
                {
                    foreach (var item in _actions.Where(item => item.Key == key))
                    {
                        Console.Clear();
                        item.Value.Action.Invoke();
                        return;
                    }

                    Console.WriteLine("Action not found");
                }
            }
        }

        private void AddItemToCollection()
        {
            if (_editingObjects[^1] is IList)
            {
                PrintEditingObject();

                Console.Write("Enter the type name: ");
                var obj = _propertyHandler.CreateObject(Console.ReadLine());

                try
                {
                    var list = (IList)_editingObjects[^1];
                    list.Add(obj);
                    Console.WriteLine("Success");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Cannot add object to the collection");
                }
            }
            else
                Console.WriteLine("This operation cannot be executed");
        }

        private void RemoveItemFromCollection()
        {
            if ((_editingObjects[^1] is IList))
            {
                var list = _editingObjects[^1] as IList;
                if (list.Count > 0)
                {

                    PrintEditingObject();

                    var index = EnterCollectionIndex(list);
                    list.RemoveAt(index);
                    Console.WriteLine("Item have been deleted");
                }
                else
                    Console.WriteLine("This collection is empty");
            }
            else
                Console.WriteLine("This operation cannot be executed");
        }

        private void EditObjectProperty()
        {
            var obj = _editingObjects[^1];
            if (obj is not IEnumerable && (!obj.GetType().IsPrimitive))
            {
                PrintEditingObject();
                var index = EnterCollectionIndex(_propertyHandler.GetProperties(obj));
                var prop = _propertyHandler.GetProperty(obj, index);
                
                Console.WriteLine("Current value: {0}", prop.GetValue(obj)!.ToString());
                Console.Write("Enter property value: ");
                var value = Console.ReadLine();

                Console.WriteLine(_propertyHandler.TrySetProperty(obj, value, prop)
                    ? "Success"
                    : "Cannot set property value");
                
            }
            else
                Console.WriteLine("This operation cannot be executed");
        }

        private void SerializeAll()
        {
            try
            {
                PrintBsonFiles();
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot read directory: {0}", e.Message);
                return;
            }

            Console.Write("Enter file name: ");
            var name = Console.ReadLine();

            try
            {
                var data = _serializer.Serialize(_editingObjects[0]);
                _filesHandler.Rewrite(data, name);
            }
            catch(Exception e)
            {
                Console.WriteLine("Serialization error: {0}", e.Message);
                return;
            }

            Console.WriteLine("Success");
        }

        private void DeserializeAll()
        {
            try
            {
                PrintBsonFiles();
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot read directory: {0}", e.Message);
                return;
            }

            Console.Write("Enter file name: ");
            var name = Console.ReadLine();
            Console.Write("Enter type of the target object");
            var typeName = Console.ReadLine();
            //var type = Type.GetType(typeName!);
            var type = typeof(List<Figure>);

            if (type == null)
            {
                Console.WriteLine("Cannot determine the type");
                return;
            }

            try
            {
                var data = _filesHandler.Read(name);
                var res = _serializer.Deserialize(data, type);
                _editingObjects.Clear();
                _editingObjects.Add(res);
            }
            catch (Exception e)
            {
                Console.WriteLine("Deserialization error: {0}", e.Message);
                return;
            }

            Console.WriteLine("Success");
        }

        private void PrintBsonFiles()
        {
            Console.WriteLine("BSON files in directory:");

            var list = _filesHandler.GetIdentifiers();
            var i = 1;
            foreach (var file in list)
            {
                Console.WriteLine("{0}. {1}", i, file);
                ++i;
            }

            Console.WriteLine();
        }

        private void InitActions()
        {
            _actions = new Dictionary<int, ActionInfo>
            {
                { 1, new ActionInfo(MoveIn, "to move in") },
                { 2, new ActionInfo(MoveOut, "to move out") },
                { 3, new ActionInfo(EditObjectProperty, "to edit object properties") },
                { 4, new ActionInfo(AddItemToCollection, "to add item to collection") },
                { 5, new ActionInfo(RemoveItemFromCollection, "to remove item from collection") },
                { 6, new ActionInfo(SerializeAll, "to serialize all") },
                { 7, new ActionInfo(DeserializeAll, "to deserialize all") },
                { 8, new ActionInfo(Exit, "to exit") }
            };
        }
    }
}
