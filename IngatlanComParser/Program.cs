using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace IngatlanComParser
{
    class Program
    {
        static void Main(string[] args)
        {
            //----------------

            Hasznaltauto.HasznaltautoParser.ParseSite();

            //----------------
            /*
            HtmlWeb web = new HtmlWeb();

            var htmlDoc = web.Load("https://ingatlan.com/szukites/elado+lakas+m2-ar-szerint+debrecen+40-mFt-ig+60-m2-felett");
            var url = "https://ingatlan.com/szukites/elado+lakas+m2-ar-szerint+debrecen+40-mFt-ig+60-m2-felett?page={0}";

            //var htmlDoc = web.Load("https://ingatlan.com/szukites/elado+lakas+m2-ar-szerint+debrecen+40-mFt-ig+60-m2-felett");

            var node = htmlDoc.DocumentNode.SelectNodes("//main[@class='resultspage__main']//div[@data-id]");
            var pageNumber = Convert.ToInt32(htmlDoc.DocumentNode.SelectSingleNode("//div[@class='pagination__page-number']").InnerText.Split(' ')[3].Trim());

            //Dictionary<string, List<EntryWithDate>> result = new Dictionary<string, List<EntryWithDate>>();
            var input = File.ReadAllText("result.json");
            var result = JsonConvert.DeserializeObject<Dictionary<string, List<EntryWithDate>>>(input);

            var entries = ParsePage(node);

            

            for (int i = 2; i < pageNumber; i++)
            {                
                entries = entries.Union(ParsePage(web, string.Format(url, i))).ToList();

                Console.WriteLine($"{i}/{pageNumber}");
            }

            foreach (var entry in entries)
            {
                if (!result.ContainsKey(entry.ID))
                {
                    result[entry.ID] = new List<EntryWithDate>() { new EntryWithDate() { Date = DateTime.UtcNow, Entry = entry } };
                    Console.WriteLine("NEW");
                    Console.WriteLine($"\t{JsonConvert.SerializeObject(entry)}");
                    Console.WriteLine("----");
                }
                else
                {
                    if (!result[entry.ID].Last().Entry.Equals(entry))
                    {
                        result[entry.ID].Add(new EntryWithDate() { Date = DateTime.UtcNow, Entry = entry });
                        Console.WriteLine("CHANGED");
                        Console.WriteLine($"\tOLD:{JsonConvert.SerializeObject(result[entry.ID].Last())}");
                        Console.WriteLine($"\tNEW:{JsonConvert.SerializeObject(result[entry.ID].Last())}");
                        Console.WriteLine("----");
                    }
                        
                }
            }

            

            var res = JsonConvert.SerializeObject(result);
            File.WriteAllText("result.json", res);
            */

        }

        public static List<Entry> ParsePage(HtmlNodeCollection nodes)
        {
            var result = new List<Entry>();
            
            foreach (var htmlNode in nodes)
            {
                var id = htmlNode.GetAttributeValue("data-id", string.Empty);
                var price = htmlNode.SelectSingleNode(".//div[@class='price']").InnerText.Trim();
                var perSquareMeterPrice = htmlNode.SelectSingleNode(".//div[@class='price--sqm']").InnerText.Trim();
                var address = htmlNode.SelectSingleNode(".//div[@class='listing__address']").InnerText.Trim();
                var areaSizeText = htmlNode.SelectSingleNode(".//div[contains(@class, 'listing__data--area-size')]").InnerText.Trim();
                var squareMeter = Convert.ToInt32(areaSizeText.Split(' ').First());

                var e = new Entry()
                {
                    ID = id,
                    Link = "",
                    Price = price,
                    PerSquareMeterPrice = perSquareMeterPrice,
                    Address = address,
                    AreaSizeText = areaSizeText,
                    SquareMeter = squareMeter
                };

                result.Add(e);
            }

            return result;
        }

        public static List<Entry> ParsePage(HtmlWeb web, string url)
        {
            var result = new List<Entry>();
            var htmlDoc = web.Load(url);

            var node = htmlDoc.DocumentNode.SelectNodes("//main[@class='resultspage__main']//div[@data-id]");

            foreach (var htmlNode in node)
            {
                var id = htmlNode.GetAttributeValue("data-id", string.Empty);
                var price = htmlNode.SelectSingleNode(".//div[@class='price']").InnerText.Trim();
                var perSquareMeterPrice = htmlNode.SelectSingleNode(".//div[@class='price--sqm']").InnerText.Trim();
                var address = htmlNode.SelectSingleNode(".//div[@class='listing__address']").InnerText.Trim();
                var areaSizeText = htmlNode.SelectSingleNode(".//div[contains(@class, 'listing__data--area-size')]").InnerText.Trim();
                var squareMeter = Convert.ToInt32(areaSizeText.Split(' ').First());

                var e = new Entry()
                {
                    ID = id,
                    Link = "",
                    Price = price,
                    PerSquareMeterPrice = perSquareMeterPrice,
                    Address = address,
                    AreaSizeText = areaSizeText,
                    SquareMeter = squareMeter
                };

                result.Add(e);
            }

            return result;
        }

        public class Entry
        {
            public string ID { get; set; }
            public string Link { get; set; }
            public string Price { get; set; }
            public string PerSquareMeterPrice { get; set; }
            public string Address { get; set; }
            public string AreaSizeText { get; set; }
            public int SquareMeter { get; set; }            

            public override bool Equals(object obj)
            {
                var y = (Entry)obj;
                return string.Equals(this.ID, y.ID) && string.Equals(this.Link, y.Link) && string.Equals(this.Price, y.Price) && string.Equals(this.PerSquareMeterPrice, y.PerSquareMeterPrice) && string.Equals(this.Address, y.Address) && string.Equals(this.AreaSizeText, y.AreaSizeText) && this.SquareMeter == y.SquareMeter;
            }
        }

        public class EntryWithDate
        {
            public DateTime Date { get; set; }
            public Entry Entry { get; set; }
        }
    }
}
