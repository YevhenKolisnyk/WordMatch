using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordMatch
{
    class Program
    {
        static readonly string[] words = System.IO.File.ReadAllLines(@"words_alpha.txt");
        private const string start = "crate";
        #region Classes

        public static Constrain GetConstrain(string original, string sample)
        {
            var bulls = original.Where((t, i) => t == sample[i]).Count();
            var letters = new List<char>();
            int cows = 0;
            for (int j = 0; j < original.Count(); j++)
                if (sample[j] != original[j])
                    letters.Add(original[j]);
            for (int i = 0; i < sample.Length; i++)
            {
                if (original[i] != sample[i] && letters.Contains(sample[i]))
                {//if not bull and present
                    letters.Remove(sample[i]);//remove first occurence
                    cows++;
                }
            }
            //int c = original.Select((t1, i) => original.Where((t, j) => t1 == sample[j] && i != j).Count()).Sum();
            return new Constrain() { Sample = sample, Bulls = bulls, Cows = cows };
        }

        public class Constrain
        {
            public int Cows { get; set; }
            public int Bulls { get; set; }
            public string Sample { get; set; }
            public bool CheckConstrain(string test)
            {
                if (test.Length != Sample.Length)
                    return false;
                var cons = GetConstrain(test, Sample);
                return this.Bulls == cons.Bulls && this.Cows == cons.Cows;
            }
        }

        private static Func<string,bool> wordIsGood(List<Constrain> restrictions) =>
            word => restrictions.TrueForAll(constrain => constrain.CheckConstrain(word));
        public static List<string> GetWords(List<Constrain> restrictions) => words.Where(wordIsGood(restrictions)).ToList();
        public static int GetWordsCount(List<Constrain> restrictions) => words.Count(wordIsGood(restrictions));

        #endregion

        #region Game logic

        public static string Guess(List<Constrain> restrictions)
        {

            var good = GetWords(restrictions);
            Console.WriteLine("Doing guess, search range: " + good.Count);

            if (good.Count == 0)
            {
                Console.WriteLine("No answer!");
                return string.Empty;
            }

            //minimax
            var min = int.MaxValue;
            var answer = good[0];
            int counter = 0;
            Console.SetCursorPosition(0, Console.CursorTop);
            foreach (var next in good)
            {
                //skip already checked
                if (restrictions.Select(x => x.Sample).Contains(next))
                    continue;
                var maxMetric = 0;
                for (int bull = 0; bull <= next.Length; bull++)
                    for (int cow = 0; cow <= next.Length - bull; cow++)
                    {
                        var cons = new Constrain() { Sample = next, Bulls = bull, Cows = cow };
                        var temp = restrictions.ToList();
                        temp.Add(cons);
                        var variantsCount = GetWordsCount(temp);
                        maxMetric = Math.Max(maxMetric, variantsCount);
                        if (maxMetric > min) goto skip;//already too big
                    }
                if (maxMetric < min)
                {
                    min = maxMetric;
                    answer = next;
                }
                skip:
                counter++;
                Console.Write((100 * counter / good.Count).ToString() + "%");
                Console.SetCursorPosition(0, Console.CursorTop);
            }
            Console.WriteLine("Guess: " + answer);
            Console.WriteLine("Accuracy: " + min);
            return answer;
        }

        #endregion

        static void Main(string[] args)
        {
            Console.WriteLine("Select a word of " + start.Length + " letters.");
            string version = start;
            var restrictions = new List<Constrain>();
            do
            {
                Console.WriteLine("My guess is:" + version);
                var cons = new Constrain() { Sample = version };
                Console.Write("Enter how many bulls(green, letters on right place):");
                cons.Bulls = System.Convert.ToInt16(Console.ReadLine());
                if (cons.Bulls == version.Length)
                    break;
                Console.Write("Enter how many cows(yellow, letters on other place):");
                cons.Cows = System.Convert.ToInt16(Console.ReadLine());
                Console.WriteLine("Version:" + version + "; green/yellow:" + cons.Bulls + "/" + cons.Cows);
                restrictions.Add(cons);
                version = Guess(restrictions);
            } while (!string.IsNullOrEmpty(version));
            if (!string.IsNullOrEmpty(version))
            {
                Console.WriteLine("--- Success! Steps=" + (restrictions.Count + 1) + "\n");
            }
            else
            {
                Console.WriteLine("--- FAIL! Steps=" + (restrictions.Count + 1) + "\n Please add word to dictionary.");
            }
            Console.ReadKey();
        }

        #region Helpers

        public static int CheckWord(string word) // verify over known word
        {
            Console.WriteLine("--------------------------------------------\n");
            Console.WriteLine("Checking word:" + word);
            var restrictions = new List<Constrain>();
            string version = start;
            do
            {
                var cons = GetConstrain(word, version);
                Console.WriteLine("Version:" + version + "; green/yellow:" + cons.Bulls + "/" + cons.Cows);
                restrictions.Add(cons);
                version = Guess(restrictions);
            } while (!string.IsNullOrEmpty(version) && version != word);
            if (version == word)
            {
                Console.WriteLine("--- Success! Steps=" + (restrictions.Count + 1) + "\n");
            }
            else
                Console.WriteLine("--- FAIL! Steps=" + (restrictions.Count + 1) + "\n");

            return restrictions.Count;
        }

        public static void CheckAllWords() // verify all words - find the best start
        {
            int maxSteps = 0;
            string hard = string.Empty;
            foreach (var word in words)
            {
                if (word.Length != start.Length) continue;
                Console.WriteLine("--------------------------------------------\n");
                Console.WriteLine("Checking word:" + word);
                var restrictions = new List<Constrain>();
                string version = start;
                do
                {
                    var cons = GetConstrain(word, version);
                    Console.WriteLine("Version:" + version + "; Bulls/Cows:" + cons.Bulls + "/" + cons.Cows);
                    restrictions.Add(cons);
                    version = Guess(restrictions);
                } while (!string.IsNullOrEmpty(version) && version != word);
                if (version == word)
                {
                    if (restrictions.Count > maxSteps)
                    {
                        maxSteps = restrictions.Count;
                        hard = word;
                    }
                    Console.WriteLine("--- Success! Steps=" + (restrictions.Count + 1) + "\n");
                }
                else
                    Console.WriteLine("--- FAIL! Steps=" + (restrictions.Count + 1) + "\n");
            }
            Console.WriteLine("Max steps=" + maxSteps);
            Console.WriteLine("Hard:" + hard);
            Console.ReadLine();
        }

        #endregion
    }
}
