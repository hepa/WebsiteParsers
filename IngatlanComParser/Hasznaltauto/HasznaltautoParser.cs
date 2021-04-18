using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace IngatlanComParser.Hasznaltauto
{
    class HasznaltautoParser
    {
        private static string OUTPUT_FILENAME = "result_hasznaltauto";
        private static string OUTPUT_EXTENSION = "json";

        public static void ParseSite()
        {
            HtmlWeb web = new HtmlWeb();            


            var htmlDoc = web.Load("https://www.hasznaltauto.hu/talalatilista/PCOG2U25V7NDADH5F76QLNGA4UXDY3V2DLJIIJSN3JVWLKBJEEPUMSK2LVAPZ5ZZNXUU2GKPOWH6H6A47MCMJ7CN3S6MQC4RFEEFRE4TWDCTMWZTS2F4YX6RUC7EAE5IQMLCEAZ5HW4B34FDOQKQN5BNKTQK7WL2E5TMGJJNYMNFY2NEDWHKOID4B3QFEZGYXDZP7U6PRGYSE5XFLYLHIKCP4BGNG4IULHPH2VZCVPAYDXKL2DJL6OCQIR62BSN6ZBY46HJ22RDQZ5BCHXVKKDZYAU4MDQDIE2ZAHE7EBFWCV6QSHNQNGCMM4BBQ6E3W5ANR27HMYHWIUWOX5T6XTOG7XU5XZTMGI7YJGDH6IHL4UPP2HZIXBA354MBZSX6SQ7PY4DAFJF3FH5PJW6XOZ33GHOUOUGTRD4BU4QJJVP4DWZ5R7ERENXZYIEKWUPLELD6XFNLYD5VIEPBX72CDV7VS72DXLMHFWFAZQ2Q6YFCLHEVNBSYXYOZM4XAG4C2BTGSHJAR6NKMVNQ76AR5YC6V4BW2IMM4FNQGWEKLOWI3WDNZWFHZD4LTCLT2PCPGG6M7F5RDY2HD4W3D546AGO4CELORWWXFKMQFI67XEHJ3AHKMSNVS6BC32UPI6GBDF5I6DDDPGJIVFFZKTW5XJR4NFDEEJB3UFB6J7OV6FPZKCTTSLSSPA54RTAVZUMUNDBJY6LPUETUDPLNDE2AGXXT6KE44KAOOH32J5XORXCKOXKVLEF5WZVYZBUA7T6ELNZHJJCHGVFG7OVHZ3X7CRIZPLFIW42RDKPS2NYDPNAV6XSICXQM3ATT33H5SLN3QP3I3QM3KC637A6KZAQESA/page1");
            var url = "https://www.hasznaltauto.hu/talalatilista/PCOG2U25V7NDADH5F76QLNGA4UXDY3V2DLJIIJSN3JVWLKBJEEPUMSK2LVAPZ5ZZNXUU2GKPOWH6H6A47MCMJ7CN3S6MQC4RFEEFRE4TWDCTMWZTS2F4YX6RUC7EAE5IQMLCEAZ5HW4B34FDOQKQN5BNKTQK7WL2E5TMGJJNYMNFY2NEDWHKOID4B3QFEZGYXDZP7U6PRGYSE5XFLYLHIKCP4BGNG4IULHPH2VZCVPAYDXKL2DJL6OCQIR62BSN6ZBY46HJ22RDQZ5BCHXVKKDZYAU4MDQDIE2ZAHE7EBFWCV6QSHNQNGCMM4BBQ6E3W5ANR27HMYHWIUWOX5T6XTOG7XU5XZTMGI7YJGDH6IHL4UPP2HZIXBA354MBZSX6SQ7PY4DAFJF3FH5PJW6XOZ33GHOUOUGTRD4BU4QJJVP4DWZ5R7ERENXZYIEKWUPLELD6XFNLYD5VIEPBX72CDV7VS72DXLMHFWFAZQ2Q6YFCLHEVNBSYXYOZM4XAG4C2BTGSHJAR6NKMVNQ76AR5YC6V4BW2IMM4FNQGWEKLOWI3WDNZWFHZD4LTCLT2PCPGG6M7F5RDY2HD4W3D546AGO4CELORWWXFKMQFI67XEHJ3AHKMSNVS6BC32UPI6GBDF5I6DDDPGJIVFFZKTW5XJR4NFDEEJB3UFB6J7OV6FPZKCTTSLSSPA54RTAVZUMUNDBJY6LPUETUDPLNDE2AGXXT6KE44KAOOH32J5XORXCKOXKVLEF5WZVYZBUA7T6ELNZHJJCHGVFG7OVHZ3X7CRIZPLFIW42RDKPS2NYDPNAV6XSICXQM3ATT33H5SLN3QP3I3QM3KC637A6KZAQESA/page{0}";
            
            var pageNumber = Convert.ToInt32(htmlDoc.DocumentNode.SelectSingleNode("//ul[@class='pagination']/li[@class='last']").InnerText.Trim());

            var result = new Dictionary<string, List<EntryWithDate>>();
            if (File.Exists(string.Join(".", OUTPUT_FILENAME, OUTPUT_EXTENSION)))
            {
                var input = File.ReadAllText(string.Join(".", OUTPUT_FILENAME, OUTPUT_EXTENSION));
                result = JsonConvert.DeserializeObject<Dictionary<string, List<EntryWithDate>>>(input);
            }
            

            var entries = ParsePage(htmlDoc);


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
            File.WriteAllText(string.Join(".", OUTPUT_FILENAME, OUTPUT_EXTENSION), res);
            File.WriteAllText(string.Join(".", OUTPUT_FILENAME, DateTime.Now.ToString("yyyyMMdd_HHmmss"), OUTPUT_EXTENSION), res);
        }

        public static List<Entry> ParsePage(HtmlWeb web, string url)
        {
            var result = new List<Entry>();
            var htmlDoc = web.Load(url);

            var nodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'row talalati-sor')]");

            foreach (var htmlNode in nodes)
            {
                var id = htmlNode.SelectSingleNode(".//a[@data-hirkod]").GetAttributeValue("data-hirkod", string.Empty);
                var title = htmlNode.SelectSingleNode(".//h3/a").InnerText.Trim();
                var link = htmlNode.SelectSingleNode(".//h3/a").GetAttributeValue("href", string.Empty);
                var price = htmlNode.SelectSingleNode(".//div[@class='vetelar']").InnerText.Trim();

                var detailsNode = htmlNode.SelectNodes(".//div[@class='talalatisor-info adatok']/span");
                //                var fuelType = detailsNode[0].InnerText.Trim();
                //                var year = detailsNode[1].InnerText.Trim();
                //                var engineCapacityCC = detailsNode[2].InnerText.Trim();
                //                var engineCapacityKW = detailsNode[3].InnerText.Trim();
                //                var engineCapacityHP = detailsNode[4].InnerText.Trim();
                //                var mileage = detailsNode[5].InnerText.Replace("&nbsp;", "").Trim();
                var details = htmlNode.SelectSingleNode(".//div[@class='talalatisor-info adatok']").InnerText.Trim();

                var e = new Entry()
                {
                    ID = id,
                    Title = title,
                    Link = link,
                    Price = price,
                    Details = details
                };

                result.Add(e);
            }

            return result;
        }

        public static List<Entry> ParsePage(HtmlDocument htmlDoc)
        {
            var result = new List<Entry>();                        

            var nodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'row talalati-sor')]");

            foreach (var htmlNode in nodes)
            {
                var id = htmlNode.SelectSingleNode(".//a[@data-hirkod]").GetAttributeValue("data-hirkod", string.Empty);
                var title = htmlNode.SelectSingleNode(".//h3/a").InnerText.Trim();
                var link = htmlNode.SelectSingleNode(".//h3/a").GetAttributeValue("href", string.Empty);
                var price = htmlNode.SelectSingleNode(".//div[@class='vetelar']").InnerText.Trim();

                var detailsNode = htmlNode.SelectNodes(".//div[@class='talalatisor-info adatok']/span");
                //                var fuelType = detailsNode[0].InnerText.Trim();
                //                var year = detailsNode[1].InnerText.Trim();
                //                var engineCapacityCC = detailsNode[2].InnerText.Trim();
                //                var engineCapacityKW = detailsNode[3].InnerText.Trim();
                //                var engineCapacityHP = detailsNode[4].InnerText.Trim();
                //                var mileage = detailsNode[5].InnerText.Replace("&nbsp;", "").Trim();
                var details = htmlNode.SelectSingleNode(".//div[@class='talalatisor-info adatok']").InnerText.Trim();

                var e = new Entry()
                {
                    ID = id,
                    Title = title,
                    Link = link,
                    Price = price,
                    Details = details
                };

                result.Add(e);
            }

            return result;
        }

        public static void OrderById()
        {
            var result = new Dictionary<string, List<EntryWithDate>>();
            if (File.Exists(OUTPUT_FILENAME))
            {
                var input = File.ReadAllText(OUTPUT_FILENAME);
                result = JsonConvert.DeserializeObject<Dictionary<string, List<EntryWithDate>>>(input);
            }

            var ordered = result.OrderBy(pair => pair.Key);

            var res = JsonConvert.SerializeObject(ordered);
            File.WriteAllText($"ordered_hasznaltauto_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}", res);
        }

        public class Entry
        {
            public string ID { get; set; }
            public string Title { get; set; }
            public string Link { get; set; }
            public string Price { get; set; }
            public string Details { get; set; }
            public string FuelType { get; set; }
            public string Year { get; set; }
            public string EngineCapacityCC { get; set; }
            public string EngineCapacityKW { get; set; }
            public string EngineCapacityHP { get; set; }
            public string Mileage { get; set; }


            protected bool Equals(Entry other)
            {
                return string.Equals(ID, other.ID) && string.Equals(Title, other.Title) && string.Equals(Link, other.Link) && string.Equals(Price, other.Price) && string.Equals(Details, other.Details) && string.Equals(FuelType, other.FuelType) && string.Equals(Year, other.Year) && string.Equals(EngineCapacityCC, other.EngineCapacityCC) && string.Equals(EngineCapacityKW, other.EngineCapacityKW) && string.Equals(EngineCapacityHP, other.EngineCapacityHP) && string.Equals(Mileage, other.Mileage);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Entry) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (ID != null ? ID.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Title != null ? Title.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Link != null ? Link.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Price != null ? Price.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Details != null ? Details.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (FuelType != null ? FuelType.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Year != null ? Year.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (EngineCapacityCC != null ? EngineCapacityCC.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (EngineCapacityKW != null ? EngineCapacityKW.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (EngineCapacityHP != null ? EngineCapacityHP.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Mileage != null ? Mileage.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }

        public class EntryWithDate
        {
            public DateTime Date { get; set; }
            public Entry Entry { get; set; }
        }

    }
}
