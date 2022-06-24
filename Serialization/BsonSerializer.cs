using System.Collections;
using System.Reflection;
using System.Text;
using Serializer.Serialization.Interfaces;

namespace Serializer.Serialization
{
    public class BsonSerializer: ISerializer
    {
        private class ObjectParseInfo
        {
            private int? NameStart { get; set; }

            private int? NameStop { get; set; }

            private int? ValueStart { get; set; }

            private int? ValueStop { get; set; }

            public string PropName { get; set; }

            public string PropValue { get; set; }


            public ObjectParseInfo() =>
                Clear();

            public void Clear()
            {
                NameStart = null;
                NameStop = null;
                ValueStart = null;
                ValueStop = null;
                PropName = null;
                PropValue = null;
            }

            public void Update(string str, int pos)
            {
                if (NameStart == null)
                    NameStart = pos + 1;
                else if (NameStop == null)
                {
                    NameStop = pos - 1;
                    PropName = str.Substring((int)NameStart, (int)NameStop - (int)NameStart + 1);
                }
                else if (ValueStart == null)
                    ValueStart = pos + 1;
                else if (ValueStop == null)
                {
                    ValueStop = pos - 1;
                    PropValue = str.Substring((int)ValueStart, (int)ValueStop - (int)ValueStart + 1);
                }
            }
        }

        private class ArrayParseInfo
        {
            private int? ItemStart { get; set; }

            private int? ItemStop { get; set; }

            public string ItemValue { get; set; }


            public ArrayParseInfo() =>
                Clear();
            


            public void Clear()
            {
                ItemStart = null;
                ItemStop = null;
                ItemValue = null;
            }

            public void Update(string str, int pos)
            {
                if (ItemStart == null)
                    ItemStart = pos + 1;
                else if (ItemStop == null)
                {
                    ItemStop = pos - 1;
                    ItemValue = str.Substring((int)ItemStart, (int)ItemStop - (int)ItemStart + 1);
                }
            }
        }


        private const string Offset = "   ";

        private const string TypePropStr = "$type";


        string ISerializer.Serialize(object obj)
        {
            string res;
            if (obj is IList)
                res = SerializeList((obj as IList), 0).ToString();
            else
                res = SerializeObject(obj, 0).ToString();

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(res));
        }

        private StringBuilder SerializeObject(object obj, int offset)
        {
            var res = new StringBuilder();
            var offsetStr = string.Join("", Enumerable.Repeat(Offset, offset));
            var innerOffsetStr = string.Concat(offsetStr, Offset);

            res.Append(offsetStr);
            res.Append("{\n");

            var typeRecord = CreateTypeRecord(obj.GetType());
            res.Append(innerOffsetStr);
            res.Append(typeRecord);
            res.Append(',');

            var props = obj.GetType().GetProperties();
            foreach (var item in props)
            {
                res.Append('\n');
                res.Append(innerOffsetStr);
                res.Append($"\"{item.Name}\" : ");

                var type = item.PropertyType;
                if (item.GetValue(obj) is IList)
                {
                    res.Append('\n');
                    res.Append(SerializeList(item.GetValue(obj) as IList, offset + 1));
                    res.Append(",\n");
                }
                else if ((!type.IsPrimitive) && (type != typeof(string)))
                {
                    res.Append('\n');
                    res.Append(SerializeObject(item.GetValue(obj), offset + 1));
                    res.Append(",\n");
                }
                else
                    res.AppendFormat("\"{0}\",", item.GetValue(obj));
            }

            if (res[res.Length - 1] == ',')
                res.Remove(res.Length - 1, 1);

            res.Append('\n');
            res.Append(offsetStr);
            res.Append('}');

            return res;
        }

        private StringBuilder SerializeList(IList obj, int offset)
        {
            var res = new StringBuilder();
            var offsetStr = string.Join("", Enumerable.Repeat(Offset, offset));
            var innerOffsetStr = string.Concat(offsetStr, Offset);

            res.Append(offsetStr);
            res.Append("[\n");

            var typeRecord = CreateTypeRecord(obj.GetType());
            res.Append(innerOffsetStr);
            res.Append(typeRecord);
            res.Append(',');

            foreach (var item in obj)
            {
                res.Append('\n');
                res.Append(innerOffsetStr);

                var type = (obj as IList).GetType().GenericTypeArguments[0];

                if (item is IList)
                {
                    res.Append('\n');
                    res.Append(SerializeList(item as IList, offset + 1));
                    res.Append(",\n");
                }
                else if ((!type.IsPrimitive) && (type != typeof(string)))
                {
                    res.Append('\n');
                    res.Append(SerializeObject(item, offset + 1));
                    res.Append(",\n");
                }
                else
                    res.Append($"\"{item}\",");
            }

            if (res[res.Length - 1] == ',')
                res.Remove(res.Length - 1, 1);

            res.Append('\n');
            res.Append(offsetStr);
            res.Append("]");
            
            return res;
        }

        object ISerializer.Deserialize(string str, Type type)
        {
            var pos = 0;
            str = Encoding.UTF8.GetString(Convert.FromBase64String(str));
            
            if (type.GetInterface(nameof(IList)) != null)
                return DeserializeArray(str, ref pos);
            else
                return DeserializeObject(str, ref pos);
        }

        private object DeserializeObject(string str, ref int pos)
        {
            var newType = ParseTypeRecord(str, ref pos);

            var info = newType.GetConstructor(Type.EmptyTypes);
            var res = info.Invoke(new object[0]);

            var pInfo = new ObjectParseInfo();

            while (pos < str.Length)
            {
                switch (str[pos])
                {
                    case '"':
                    {
                        pInfo.Update(str, pos);
                        if (pInfo.PropValue != null)
                        {
                            var prop = newType.GetProperty(pInfo.PropName!);
                            prop!.SetValue(res, Convert.ChangeType(pInfo.PropValue, prop.PropertyType), null);
                            pInfo.Clear();
                        }

                        break;
                    }

                    case '{':
                    {
                        if (pInfo.PropName != null)
                        {
                            var propObj = newType.GetProperty(pInfo.PropName);
                            var propObjValue = DeserializeObject(str, ref pos);
                            propObj!.SetValue(res, propObjValue);
                            pInfo.Clear();
                        }

                        break;
                    }

                    case '}':
                    {
                        return res;
                    }

                    case '[':
                    {
                        if (pInfo.PropName != null)
                        {
                            var propObj = newType.GetProperty(pInfo.PropName);
                            var propObjValue = DeserializeArray(str, ref pos);
                            propObj!.SetValue(res, propObjValue);
                            pInfo.Clear();
                        }
                        else
                            throw new Exception("Deserialization parsing error");

                        break;
                    }
                }

                ++pos;
            }

            throw new Exception("Deserialization error");
        }

        private object DeserializeArray(string str, ref int pos)
        {
            var newType = ParseTypeRecord(str, ref pos);
            var info = newType.GetConstructor(Type.EmptyTypes);
            var res = info!.Invoke(Array.Empty<object>());

            if (res is IList)
            {

                var pInfo = new ArrayParseInfo();

                while (pos < str.Length)
                {
                    switch (str[pos])
                    {
                        case '"':
                        {
                            pInfo.Update(str, pos);
                            if (pInfo.ItemValue != null)
                            {
                                var t = Type.GetTypeCode(((res as IList)!).GetType().GenericTypeArguments[0]);
                                ((res as IList)!).Add(Convert.ChangeType(pInfo.ItemValue, t));
                                pInfo.Clear();
                            }

                            break;
                        }

                        case ']':
                        {
                            return res;
                        }

                        case '{':
                        {
                            var newItem = DeserializeObject(str, ref pos);
                            ((res as IList)!).Add(newItem);
                            pInfo.Clear();

                            break;
                        }

                        case '[':
                        {
                            if (pInfo.ItemValue != null)
                            {
                                var newItem = DeserializeArray(str, ref pos);
                                ((res as IList)!).Add(newItem);
                                pInfo.Clear();
                            }

                            break;
                        }
                    }

                    ++pos;
                }

                throw new Exception("Deserialization error");
            }
            else
                return res;
        }

        private static StringBuilder CreateTypeRecord(Type type)
        {
            var res = new StringBuilder();
            res.Append('"');
            res.Append(TypePropStr);
            res.Append("\" : \"");
            res.Append(type);
            res.Append('"');
            return res;
        }

        private static Type ParseTypeRecord(string str, ref int pos)
        {
            var info = new ObjectParseInfo();

            while (pos < str.Length)
            {
                if (str[pos] == '"')
                {
                    info.Update(str, pos);
                    if ((info.PropName != null) && (info.PropName.Equals(TypePropStr)) && (info.PropValue != null))
                    {
                        ++pos;
                        return Type.GetType(info.PropValue)!;
                    }
                }

                ++pos;
            }

            return null;
        }
    }
}
