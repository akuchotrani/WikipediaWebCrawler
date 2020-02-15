using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Linq;
using HtmlAgilityPack;

namespace WikipediaWebCrawler
{
    class MainClass
    {

        /// <summary>
        /// Tokenizes the string using regular expression.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>returns an array of string of tokens</returns>
        public static string[] TokenizeString(string data)
        {
            string[] words = Regex.Split(data.ToLower(), @"\W+");
            return words;
        }


        /// <summary>
        /// Creates a hashmap for every word and their frequency.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns>returns a dictionary of word frequency</returns>
        public static Dictionary<string, int> BuildCounter(string[] tokens)
        {
            Dictionary<string, int> counter = new Dictionary<string, int>();
            foreach(var token in tokens)
            {
                if (counter.ContainsKey(token))
                {
                    counter[token] += 1;
                }
                else
                {
                    counter[token] = 1;
                }
            }
            return counter;
        }


        /// <summary>
        /// Gets the html of the requested URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns>string representation of the html</returns>
        public static string GetRequestHtml(string url)
        {
            var client = new WebClient();
            try
            {
                var text = client.DownloadString("https://en.wikipedia.org/wiki/Microsoft");
                return text;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Message: ", ex.Message);
                return "";
            }
        }

        /// <summary>
        /// Gets all the elements on the webpage
        /// Checks if the current element tag is H2 and the section name is within the inner text
        /// Records the text of that section
        /// </summary>
        /// <param name="html"></param>
        /// <param name="sectionName"></param>
        /// <returns>returns the text of a particular section</returns>
        public static string GetWikiSectionHtml(string html, string sectionName)
        {
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(html);
            string sectionText = "";
            bool isSectionFound = false;
            foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes("//*"))
            {
                if (node.Name == "h2")
                {
                    if (node.InnerText == sectionName)
                        isSectionFound = true;
                    else
                        isSectionFound = false;
                }

                if (isSectionFound)
                {
                    sectionText += " " + node.InnerText;
                }
            }

            return sectionText;
        }

            

        public static void Main(string[] args)
        {

            while (true)
            {
                
                Console.WriteLine("How many TOP tokens you need");
                var userInput = Console.ReadLine();
                int top = 0;
                bool result = int.TryParse(userInput, out top);
                if (result == false)
                {
                    Console.WriteLine("Enter a valid integer and try again!");
                    continue;
                }


                Console.WriteLine("Do you want to exclude certain keywords from result? press Y/N");
                HashSet<string> blockList = new HashSet<string>();
                string exludeOption = Console.ReadLine();
                if (exludeOption == "y" || exludeOption == "Y")
                {
                    Console.WriteLine("Enter space separated list of tokens to exclude");
                    string tokensToExclude = Console.ReadLine();
                    string[] wordsToExclude = TokenizeString(tokensToExclude);
                    foreach (var word in wordsToExclude)
                    {
                        blockList.Add(word);
                    }

                }

                var url = "https://en.wikipedia.org/wiki/Microsoft";
                var html = GetRequestHtml(url);
                var sectionText = GetWikiSectionHtml(html, "History");
                var tokens = TokenizeString(sectionText);
                Dictionary<string, int> counter = BuildCounter(tokens);


                //Sorting all the words by their frequency
                var items = from pair in counter
                            orderby pair.Value descending
                            select pair;

                //Displaying relevant result
                int displayTokens = 0;
                foreach (var kv in items)
                {
                    if (displayTokens == top)
                        break;

                    int temp = 0;
                    bool isNumber = int.TryParse(kv.Key, out temp);
                    if (blockList.Contains(kv.Key) || isNumber)
                    {
                        continue;
                    }

                    Console.WriteLine("Word: " + kv.Key + " \tOccurance:" + kv.Value);
                    displayTokens += 1;
                }

                //Checking if the user wants to terminate the application.
                Console.WriteLine("Do you want to exit? Press Y/N");
                var exitInput = Console.ReadLine();
                if (exitInput == "y" || exitInput == "Y")
                    break;
            }
        }
    }
}
