using gijergre_generator.Indexing;

namespace gijergre_generator
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Indexer indexer = new Indexer("hello.awesome");
            Console.WriteLine(indexer.GetGijergre("Hello"));
            Console.WriteLine(indexer.GetGijergre("Hello") + " " + indexer.GetGijergre("Goodbye"));
            Console.WriteLine(indexer.GetEnglish(indexer.GetGijergre("Hello")));
        }
    }
}