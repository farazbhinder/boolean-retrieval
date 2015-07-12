using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/*

    Faraz Ahmad
    
    Boolean Retrieval

*/

namespace ConsoleApplication2
{
    class Program
    {
        //we tokenize the words on these punctuations
        static char[] punctuations = new Char[] { ' ', ',', '.', ';', '^', '`', ':', '?', '&', '!', '+', '-', '_', '#', '<', '>', '/', '|', '\\', '"', '(', ')', '[', ']', '=', '*', '%','\t' };
        
        const int THRESHOLD = 25000;

        //initial dictionary - it stores the term frequencies...
        //format is Dictionary<term x <doc y, tf>>
        static Dictionary<string, HashSet<string>> dt = new Dictionary<string, HashSet<string>>();

        static HashSet<string> allDocuments = new HashSet<string>();

        static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                return 0;
            }

            string query = args[0];
            string path = args[1];


            Stopwatch stopWatch = new Stopwatch();
            try
            {
                stopWatch.Start();
                MakeIndex(path);
                stopWatch.Stop();
                //Console.WriteLine(stopWatch.Elapsed);

                RunQuery(query);


            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }

            
            //Dictionary<string, HashSet<string>> dt = null;
            //using(Stream stream = File.OpenRead("binaryindex"))
            //{
            //    BinaryFormatter deserializer = new BinaryFormatter();
            //    dt = (Dictionary<string, HashSet<string>>)deserializer.Deserialize(stream);
            //} 


            

            Console.ReadLine();
            return 0;
        }

        static void MakeIndex(string sDir)
        {
            HashSet<string> stopwordslist = GetStopWords();
            HashSet<string> noDuplicates = new HashSet<string>();
            Dictionary<string, int> tokensOccurences = new Dictionary<string, int>();
            foreach (string filename in Directory.EnumerateFiles(sDir, "*.*", SearchOption.AllDirectories))
            {
                //foreach (string filename in Directory.GetFiles(d))
                //{
                    StreamReader reader = File.OpenText(filename);
                    allDocuments.Add(filename);
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] items = line.Split(punctuations);
                        foreach (string s in items)
                        {
                            if (string.IsNullOrWhiteSpace(s))
                            {
                                continue;
                            }
                            //make a term from this token...
                            string st = s.ToLower();
                            st = GetStemmedTerm(st);

                            int count = 0;
                            if (tokensOccurences.TryGetValue(st, out count) == true)
                            {
                                tokensOccurences[st] = count + 1;
                            }
                            else
                            {
                                tokensOccurences.Add(st, 1);
                            }

                            //dont index the stop words
                            if (!stopwordslist.Contains(st) && !Regex.IsMatch(st, @"^\d+$"))
                            {
                                if (noDuplicates.Add(st))
                                {//this term doesn't exist in the hashtable/dictionary dt
                                    HashSet<string> temp = new HashSet<string>();
                                    temp.Add(filename);
                                    dt.Add(st, temp);
                                }
                                else
                                {//this term already exist in the hashtable/dictionary dt
                                    dt[st].Add(filename);
                                }
                            }
                        }

                    }
                    reader.Close();

                //}
            }
            var sortedDict = from entry in tokensOccurences orderby entry.Value descending select entry;
            Dictionary<string, int> tokenOccurences2 = sortedDict.ToDictionary(p => p.Key, p => p.Value);

            int i = 0;
            //frequency thresholding
            foreach (KeyValuePair<string, int> kvp in tokenOccurences2)
            {
                if (kvp.Value > THRESHOLD)
                {
                    dt.Remove(kvp.Key);
                    i++;
                }
                else
                {
                    break;
                }
            }
        }

        static void RunQuery(string query)
        {
            string[] words = query.Split(punctuations);
            if (words.Length == 1)
	        {
                string st = words[0].ToLower();
                st = GetStemmedTerm(st);
                HashSet<string> outp = new HashSet<string>();
                if (dt.TryGetValue(st, out outp))
                {
                    //Console.WriteLine("Found {0} documents:", outp.Count);
                    //foreach (string item in outp)
                    //{
                    //    Console.WriteLine("{0}", item);
                    //}
                    PrintResults(outp);
                }
                else
                {
                    Console.WriteLine("No documents found...");
                }
	        }
            else if (words.Length == 2)
            {//NOT term1
                if (words[0].ToUpper() == "NOT")
                {
                    string st = words[1].ToLower();
                    st = GetStemmedTerm(st);
                    HashSet<string> outp = new HashSet<string>();
                    if (dt.TryGetValue(st, out outp)){}
                    outp = new HashSet<string>(allDocuments.Except(outp));
                    if (outp.Count>0)
                    {
                        //Console.WriteLine("Found {0} documents:", outp.Count);
                        //foreach (string item in outp)
                        //{
                        //    Console.WriteLine("{0}", item);
                        //}
                        PrintResults(outp);
                    }
                    else
                    {
                        Console.WriteLine("No documents found...");
                    }
                }
            }
            else if (words.Length == 3)
            {//term1 AND/OR term2
                HashSet<string> outp = new HashSet<string>();

                string st1 = words[0].ToLower();
                st1 = GetStemmedTerm(st1);
                HashSet<string> outp1 = new HashSet<string>();

                string st2 = words[2].ToLower();
                st2 = GetStemmedTerm(st2);
                HashSet<string> outp2 = new HashSet<string>();

                if (dt.TryGetValue(st1, out outp1)){}

                if (dt.TryGetValue(st2, out outp2)){}

                if (words[1].ToUpper() == "AND")
                {
                    outp = new HashSet<string>(outp1.Intersect(outp2));
                }
                else if (words[1].ToUpper() == "OR")
                {
                    outp = new HashSet<string>(outp1.Union(outp2));
                }

                if (outp.Count > 0)
                {
                    //Console.WriteLine("Found {0} documents:", outp.Count);
                    //foreach (string item in outp)
                    //{
                    //    Console.WriteLine("{0}", item);
                    //}
                    PrintResults(outp);
                }
                else
                {
                    Console.WriteLine("No documents found...");
                }
            }
        }

        static void PrintResults(HashSet<string> outp)
        {
            //Console.WriteLine("Found {0} documents:", outp.Count);

            foreach (string item in outp)
            {

                string fullfilepath = item.ToString();
                string[] filesfoldersnames = fullfilepath.Split(new char[] { '\\' });
                string pathIn20NewsgroupFolder = "";
                for (int j = filesfoldersnames.Length - 2; j < filesfoldersnames.Length; j++)
                {
                    pathIn20NewsgroupFolder = pathIn20NewsgroupFolder + "/" + filesfoldersnames[j];
                }

                //Console.WriteLine(dslist[i].ToString());
                Console.WriteLine(pathIn20NewsgroupFolder.Substring(1));
                
                //Console.WriteLine("{0}", item);
            }
        }

        static HashSet<string> GetStopWords()
        {
            HashSet<string> stopwords = new HashSet<string>();
            StreamReader sr = File.OpenText("stopwords1.txt");
             string line;
             while ((line = sr.ReadLine()) != null)
             {
                 if(line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                 { continue; }
                 stopwords.Add(line.Trim());
                 //Console.WriteLine(line);
             }
             sr.Close();
             return stopwords;

        }

        static string GetStemmedTerm(string st)
        {
            Stemmer stemmer = new Stemmer();
            char[] starr = st.ToCharArray();
            stemmer.add(starr, starr.Length);
            stemmer.stem();
            return stemmer.ToString();
        }
    }
}
