using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace GoogleFetch {
    /// <summary>
    /// Crawler Google SERP.
    /// This class was inspired in seer.js-public project by Chris Le.
    /// </summary>
    /// <see cref="http://www.seerinteractive.com/blog/google-scraper-in-google-docs-update"/>
    /// <see cref="https://github.com/chrisle/seer.js-public/blob/master/google_scraper.js"/>
    public static class GoogleScraper {

        //static bool _errorOccurred;

        /// <summary>
        /// Gets stuff inside two tags
        /// </summary>
        /// <param name="haystack">haystack String to look into</param>
        /// <param name="start">start Starting tag</param>
        /// <param name="end">end Ending tag</param>
        /// <returns>Stuff inside the two tags</returns>
        private static string getInside(string haystack, string start, string end) {
            var startIndex = haystack.IndexOf(start) + start.Length;
            var endIndex = haystack.IndexOf(end);
            return haystack.Substring(startIndex, endIndex - startIndex);
        }


        /// <summary>
        /// Fetch keywords from Google.  Returns error message if an error occurs.
        /// </summary>
        /// <param name="wc"> </param>
        /// <param name="kw">kw Keyword</param>
        /// <param name="optResults">params Extra parameters as an array of key, values.</param>
        /// <returns></returns>
        private static string fetch(WebClient wc, string kw, int optResults = 10) {
            //var errorOccurred = false;
            //optResults = optResults || 10;
            try {
                var url = "http://www.google.com.br/search?q=" + kw + "&num=" + optResults;
                return wc.DownloadString(url);
                //return await _UrlFetchApp.DownloadStringTaskAsync(url);
            }
            catch (Exception e) {
                //_errorOccurred = true;
                throw e;
            }
        }


        /// <summary>
        /// Extracts the URL from an organic result. Returns false if nothing is found.
        /// </summary>
        /// <param name="result">result XML string of the result</param>
        /// <returns></returns>
        private static string extractUrl(string result) {
            var url = string.Empty;
            //Regex er = new Regex();
            //if (er.IsMatch(result)){
            
            if (Regex.Match(result,@"\/url\?q=").Success){
                url = getInside(result, "?q=", "&amp");
                return (url != string.Empty) ? url : string.Empty;//false;
            }
            return string.Empty;
        }


        /// <summary>
        /// Extracts the organic results from the page and puts them into an array.
        /// One per element.  Each element is an XMLElement.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private static string[] extractOrganic(string html) {
            html = Regex.Replace(html, @"\n|\r", "", RegexOptions.Multiline);
            var allOrganic = Regex.Matches(html, @"<li class=""g"">(.*)<\/li>",
                                           RegexOptions.Multiline | RegexOptions.IgnoreCase);

            var results = allOrganic[0].Value.Split(new string[] { "<li class=\"g\">" }, StringSplitOptions.None);
            var i = 0;
            var len = results.Length;
            string url = string.Empty;
            var organicData = new List<string>();
            while (i < len) {
                url = extractUrl(results[i]);
                if (url.Length > 0 && url.IndexOf("http") == 0) {
                    organicData.Add(url);
                }
                i++;
            }
            return organicData.ToArray();
        }


        public static string[] Get(string kw, int optResults) {
            using (var wc = new WebClient()) {
                var result = fetch(wc, kw, optResults);
                return extractOrganic(result);
            }
        }
    }
}
