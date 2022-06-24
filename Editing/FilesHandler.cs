using System.Text;
using Serializer.Editing.Interfaces;

namespace Serializer.Editing
{
    public class FilesHandler: IFilesHandler
    {
        public string FilePath { get; set; }


        public FilesHandler(string filePath) =>
            FilePath = filePath;


        string IFilesHandler.Read(string identifier)
        {
            var path = string.Concat(FilePath, "\\", identifier);

            using var reader = new StreamReader(path);
            return reader.ReadToEnd();
        }

        void IFilesHandler.Append(string data, string identifier)
        {
            var path = string.Concat(FilePath, "\\", identifier);
            if (File.Exists(path))
                throw new Exception("This file already exists");

            using var writer = new StreamWriter(path, true);
            writer.Write(data);
        }

        void IFilesHandler.Rewrite(string data, string identifier)
        {
            var path = string.Concat(FilePath, "\\", identifier);
            using var writer = new StreamWriter(path, false);
            writer.Write(data);
        }
            

        List<string> IFilesHandler.GetIdentifiers() =>
            Directory.GetFiles(FilePath).ToList().Select(file => file.Remove(0, FilePath.Length + 1)).ToList();

    }
}
