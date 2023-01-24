using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gijergre_generator.Indexing
{
    internal class Index
    {
        public Dictionary<char, Index> Layer { get; set; } = new Dictionary<char, Index>();

        public string Alternative { get; set; } = string.Empty;

        public Index() {}

        public string FindValue(char[] key)
        {
            if(key.Length < 1) { return Alternative; }
            if (!Layer.ContainsKey(key[0])) { return string.Empty; }
            if (Layer[key[0]] == null) { return string.Empty; }
            char[] newKey = new char[key.Length-1];
            Array.Copy(key, 1, newKey, 0, newKey.Length);
            return Layer[key[0]].FindValue(newKey);

        }

        public void IndexWord(char[] word, string result)
        {
            if (word.Length < 1) { Alternative = result; return; }
            if (!Layer.ContainsKey(word[0])) Layer.Add(word[0], new Index());
            char[] newWord = new char[word.Length-1];
            Array.Copy(word, 1, newWord, 0, newWord.Length);
            Layer[word[0]].IndexWord(newWord, result);
        }
    }
}
