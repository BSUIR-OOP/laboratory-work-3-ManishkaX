namespace Serializer.Serialization.Interfaces
{
    public interface ISerializer
    {
        string Serialize(object obj);

        object Deserialize(string str, Type type);
    }
}
