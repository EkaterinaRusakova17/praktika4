using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace TextEditorApp
{ 
    public class TextFile
    {
        public string FileName { get; set; }
        public string Content { get; set; }

        public void SerializeToBinary(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, this);
            }
        }

        public static TextFile DeserializeFromBinary(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (TextFile)formatter.Deserialize(fs);
            }
        }

        public void SerializeToXml(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TextFile));
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, this);
            }
        }

        public static TextFile DeserializeFromXml(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TextFile));
            using (StreamReader reader = new StreamReader(filePath))
            {
                return (TextFile)serializer.Deserialize(reader);
            }
        }
    }

    public static class TextFileSearcher
    {
        public static IEnumerable<string> SearchFiles(string directoryPath, params string[] keywords)
        {
            List<string> foundFiles = new List<string>();

            if (Directory.Exists(directoryPath))
            {
                foreach (string filePath in Directory.GetFiles(directoryPath, "*.txt", SearchOption.AllDirectories))
                {
                    string content = File.ReadAllText(filePath);
                    if (keywords.All(keyword => content.Contains(keyword)))
                    {
                        foundFiles.Add(filePath);
                    }
                }
            }

            return foundFiles;
        }
    }

    public class TextEditor
    {
        private TextFile textFile;
        private Stack<TextFileMemento> history;

        public TextEditor(TextFile textFile)
        {
            this.textFile = textFile;
            this.history = new Stack<TextFileMemento>();
            SaveMemento();
        }

        public void MakeChange(string newText)
        {
            textFile.Content = newText;
            SaveMemento();
        }

        public void Undo()
        {
            if (history.Count > 0)
            {
                TextFileMemento previousState = history.Pop();
                textFile.Content = previousState.Content;
            }
        }

        public override string ToString()
        {
            return textFile.Content;
        }

        private void SaveMemento()
        {
            history.Push(new TextFileMemento(textFile.Content));
        }
    }

    public class TextFileMemento
    {
        public string Content { get; }

        public TextFileMemento(string content)
        {
            Content = content;
        }
    }

    public static class TextFileIndexer
    {
        public static Dictionary<string, List<string>> IndexFiles(string directoryPath, params string[] keywords)
        {
            Dictionary<string, List<string>> index = new Dictionary<string, List<string>>();

            if (Directory.Exists(directoryPath))
            {
                foreach (string filePath in Directory.GetFiles(directoryPath, "*.txt", SearchOption.AllDirectories))
                {
                    string content = File.ReadAllText(filePath);
                    foreach (string keyword in keywords)
                    {
                        if (content.Contains(keyword))
                        {
                            if (!index.ContainsKey(keyword))
                            {
                                index[keyword] = new List<string>();
                            }

                            index[keyword].Add(filePath);
                        }
                    }
                }
            }

            return index;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            TextFile textFile = new TextFile()
            {
                FileName = "example.txt",
                Content = "Hello, world!"
            };

            TextEditor editor = new TextEditor(textFile);
            Console.WriteLine("Initial content:");
            Console.WriteLine(editor.ToString());

            editor.MakeChange("Hello, Universe!");
            Console.WriteLine("Changed content:");
            Console.WriteLine(editor.ToString());

            editor.Undo();
            Console.WriteLine("Undo change:");
            Console.WriteLine(editor.ToString());

            var indexedFiles = TextFileIndexer.IndexFiles("directoryPath", "keyword1", "keyword2");
            Console.WriteLine("Indexed files:");
            foreach (var item in indexedFiles)
            {
                Console.WriteLine($"Keyword: {item.Key}");
                foreach (var file in item.Value)
                {
                    Console.WriteLine($"File: {file}");
                }
            }
        }
    }
}
