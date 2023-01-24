using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace gijergre_generator.Indexing
{
    internal class Indexer
    {
        //random number for signature.
        private const ulong Signature = (ulong)0x3853235453;
        //Indexes
        private Index Forwards = new Index();
        private Index Backwards = new Index();

        public char[] GijergreTable = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '[', '}', '{', ']', '@', '#', ';', '/', ',', '`', '-', '=' };

        public Dictionary<string, string> Dictionary { get; private set; } = new Dictionary<string, string>();

        private Random randomGenerator = new Random();

        //File Structure:
        private uint CurrentDictionaryLength = 0;
        private FileStream File { get; set; }
        private BinaryReader Reader { get; set; }
        private BinaryWriter Writer { get; set; }
        private object StreamLock = new object();

        public Indexer(string fileToSave)
        {
            ReadyStream(fileToSave);
        }

        public string GetGijergre(string English)
        {
            if (string.IsNullOrWhiteSpace(English)) return string.Empty;
            return FindOrCreateGijergre(English);
        }

        public string GetEnglish(string Gijergre)
        {
            if (string.IsNullOrWhiteSpace(Gijergre))
                return string.Empty;
            return FindEnglish(Gijergre);
        }

        private string FindEnglish(string gijergre)
        {
            char[] gijergreChars = gijergre.ToCharArray();
            string english = Backwards.FindValue(gijergreChars);
            if (string.IsNullOrEmpty(english))
                return string.Empty;
            return english;
        }

        private string FindOrCreateGijergre(string english)
        {
            char[] englishChars = english.ToCharArray();
            string gijergre = Forwards.FindValue(englishChars);
            if (string.IsNullOrEmpty(gijergre))
            {
                gijergre = GenerateGijigre();
                AddToDictionaryIndex(english, gijergre);
                SaveChanges();
            }
            return gijergre;
        }

        private string GenerateGijigre()
        {
            int length = randomGenerator.Next(30);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                builder.Append(GijergreTable[randomGenerator.Next(GijergreTable.Length-1)]);
            }
            return builder.ToString();
        }

        private void IndexWord(string english, string gijergre)
        {
            char[] englishChars = english.ToCharArray();
            char[] gijergreChars = gijergre.ToCharArray();
            for (int i = 0; i < englishChars.Length; i++)
            {
                Forwards.IndexWord(englishChars, gijergre);
                Backwards.IndexWord(gijergreChars, english);
            }
        }

        private void AddToDictionaryIndex(string english, string gijergre)
        {
            if (string.IsNullOrEmpty(english) == true || string.IsNullOrEmpty(gijergre))
                throw new InvalidOperationException("Cannot index empty strings");
            Dictionary.Add(english, gijergre);
            IndexWord(english, gijergre);
        }
        private bool InBounds(int index) => index > -1 && index < Dictionary.Count;

        #region Serialisation

        private void SaveChanges()
        {
            lock (StreamLock)
            {
                if (!VerifySignature())
                    throw new InvalidOperationException("File has not been staged correctly");
                Writer.Write(Dictionary.Count);
                foreach ((string english, string gijergre) in Dictionary)
                {
                    StringWrite(english);
                    StringWrite(gijergre);
                }
                Writer.Flush();
                if (GetSavedLength() != Dictionary.Count())
                    throw new FileLoadException("File has written incorrectly.");
            }
        }


        private void ReadyStream(string file)
        {
            //If the file 
            if (File != null)
                File.Dispose();

            File = System.IO.File.Open(file, FileMode.OpenOrCreate);
            Reader = new BinaryReader(File);
            Writer = new BinaryWriter(File);

            if (VerifySignature())
            {
                FillDictionary();
            }
            else
            {
                WriteSignature();
                lock (StreamLock)
                {
                    Writer.Write((ulong)0);
                    File.Flush();
                }
            }
        }



        //Typically one time use.
        private void FillDictionary()
        {
            lock (StreamLock)
            {
                uint length = GetSavedLength(true);
                for (uint i = 0; i < length; i++)
                {
                    //Get English
                    string english = StringRead();
                    //Get Gijergre alternative
                    string gijergre = StringRead();
                    AddToDictionaryIndex(english, gijergre);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="positionCorrect">If set to true, it will not verify the position of the signature before reading, careful</param>
        private uint GetSavedLength(bool positionCorrect = false)
        {
            lock (StreamLock)
            {
                if (!positionCorrect)
                {
                    File.Position = 0;
                    if (!VerifySignature())
                        throw new InvalidOperationException("File not staged correctly");
                }
                uint length = Reader.ReadUInt32();
                CurrentDictionaryLength = length;
                return length;
            }
        }

        private void StringWrite(string stringToWrite)
        {
            lock (StreamLock)
            {
                uint length = (uint)stringToWrite.Length;
                char[] chars = stringToWrite.ToCharArray();

                Writer.Write(length);
                for (int i = 0; i < length; i++)
                {
                    Writer.Write(chars[i]);
                }
                File.Flush();
            }
        }

        private string StringRead()
        {
            lock (StreamLock)
            {
                uint length = Reader.ReadUInt32();
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < length; i++)
                {
                    char character = Reader.ReadChar();
                    builder.Append(character);
                }

                return builder.ToString();
            }
        }

        private void WriteSignature()
        {
            lock (StreamLock)
            {
                File.Position = 0;
                Writer.Write(Signature);
                File.Flush();
            }
        }
        private bool VerifySignature()
        {
            lock (StreamLock)
            {
                File.Position = 0;
                if (File.Length < sizeof(ulong)) return false;
                ulong signature = Reader.ReadUInt64();
                return signature.Equals(Signature);
            }
        }

        #endregion
    }
}
