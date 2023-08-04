using StratenAanzet.Domain;
namespace StratenAanzet.Cui
{
    public class StratenAanzetApp
    {
        private readonly string file;
        private Dictionary<string, Dictionary<string, SortedSet<string>>> totaleData;

        public void Start()
        {

            Console.WriteLine("Starting app...");
            System.Diagnostics.Debug.WriteLine("We starten de File Processor...");

            var myFileProcessor = new FileProcessor("C:\\Users\\andy\\OneDrive - Hogeschool Gent\\Bureaublad\\StratenAanzet Tussentijdse Evaluatie", "straatnamen", "straatnamen");
            myFileProcessor.Unzip("AdresBestanden.zip");

            myFileProcessor.ReadFiles(new List<string> {
                "straatnamen.csv", // index: 0
                "Gemeentenaam.csv", // index: 1
                "StraatnaamID_gemeenteID.csv", // index: 2
                "ProvincieInfo.csv", // index: 3
                "ProvincieIDsVlaanderen.csv" // index: 4
            });

            myFileProcessor.VoegToeAanDictionary();
            totaleData = myFileProcessor.GeefDictionaryTerug();

            foreach (var provincie in totaleData)
            {
                Console.WriteLine($"- {provincie.Key}:");
                foreach (var gemeente in provincie.Value)
                {
                    Console.WriteLine($"\t{gemeente.Key}:");
                    foreach (var straat in gemeente.Value)
                    {
                        Console.WriteLine($"\t\t{straat}");
                    }
                }
            }

            myFileProcessor.MaakMappenAan();
        }
    }
}