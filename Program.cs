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
            var dogs = original.Where((t, i) => t == sample[i]).Count();
            var letters = new List<char>();
            int cats = 0;
            for (int j = 0; j < original.Count(); j++)
                if (sample[j] != original[j])
                    letters.Add(original[j]);
            for (int i = 0; i < sample.Length; i++)
            {
                if (original[i] != sample[i] && letters.Contains(sample[i]))
                {//if not dog and present
                    letters.Remove(sample[i]);//remove first occurence
                    cats++;
                }
            }
            //int c = original.Select((t1, i) => original.Where((t, j) => t1 == sample[j] && i != j).Count()).Sum();
            return new Constrain() { Sample = sample, Dogs = dogs, Cats = cats };
        }

        public class Constrain
        {
            public int Cats { get; set; }
            public int Dogs { get; set; }
            public string Sample { get; set; }
            public bool CheckConstrain(string test)
            {
                if (test.Length != Sample.Length)
                    return false;
                var cons = GetConstrain(test, Sample);
                return this.Dogs == cons.Dogs && this.Cats == cons.Cats;
            }
        }

        public static List<string> GetWords(List<Constrain> restrictions)
        {
            return words.Where(w => restrictions.TrueForAll(constrain => constrain.CheckConstrain(w))).ToList();
        }

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
            var answers = new List<string>();
            int counter = 0;
            Console.SetCursorPosition(0, Console.CursorTop);
            foreach (var next in good)
            {
                //skip already checked
                if (restrictions.Select(x => x.Sample).Contains(next))
                    continue;
                var maxMetric = 0;
                for (int dog = 0; dog <= next.Length; dog++)
                    for (int cat = 0; cat <= next.Length - dog; cat++)
                    {
                        var cons = new Constrain() { Sample = next, Dogs = dog, Cats = cat };
                        var temp = restrictions.ToList();
                        temp.Add(cons);
                        var variants = GetWords(temp);
                        maxMetric = Math.Max(maxMetric, variants.Count);
                    }
                if (maxMetric < min)
                {
                    min = maxMetric;
                    answer = next;
                    //answers.Clear();
                    //answers.Add(next);
                }
                else if (maxMetric == min)
                    answers.Add(next);
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
                cons.Dogs = System.Convert.ToInt16(Console.ReadLine());
                if (cons.Dogs == version.Length)
                    break;
                Console.Write("Enter how many cows(yellow, letters on other place):");
                cons.Cats = System.Convert.ToInt16(Console.ReadLine());
                Console.WriteLine("Version:" + version + "; green/yellow:" + cons.Dogs + "/" + cons.Cats);
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
                Console.WriteLine("Version:" + version + "; green/yellow:" + cons.Dogs + "/" + cons.Cats);
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
                    Console.WriteLine("Version:" + version + "; Dogs/Cats:" + cons.Dogs + "/" + cons.Cats);
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
