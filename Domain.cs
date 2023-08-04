using System.Diagnostics;
using System.IO.Compression;

namespace StratenAanzet.Domain
{
    public class FileProcessor
    {
        #region Lists
        readonly List<string> provincieIds = new(); // 1,2,4,5,8
        readonly List<ProvincieGemeente> gemeenteProvincieLinks = new(); // gemeente id en provincieId aanwezig  provincienaam onder v
        readonly List<ProvincieGemeente> gemeenteGemeenteLinks = new(); // gemeente id en Gemeentenaam aanwezig onder v
        readonly List<ProvincieGemeente> gemeenteLinks = new(); // gemeente id en straatnaam id aanwezig 
        readonly List<GemeenteStraat> straatLinks = new(); // straatnaam id en straatnaam aanwezig
        #endregion
        #region Fields
        private readonly string _path;
        private readonly string _extractPath;
        private string _resultPath { get; set; }
        #endregion
        #region Ctor
        public FileProcessor(string path, string extractPath, string resultPath)
        {
            _path = path;
            _extractPath = extractPath;
            _resultPath = resultPath;
        }
        #endregion
        private Dictionary<string, Dictionary<string, SortedSet<string>>> totaleData;

        public Dictionary<string, Dictionary<string, SortedSet<string>>> GeefDictionaryTerug() // dit geeft de volledige dictionary terug om te gebruiken in de CUI
        {
           return totaleData;
        }

        #region Methods files
        public void ReadFiles(List<string> fileNames)
        {
            using (var stream = new StreamReader(Path.Combine(_path, _extractPath, fileNames[4]))) // opent een bestand "ProvincieIDsVlaanderen.csv" // index: 4
            {

                var ids = stream?.ReadLine()?.Trim()?.Split(',');
                // string line = ids.ToString();
                if (ids != null && ids.Length > 0)
                {
                    foreach (var id in ids)
                    { // hierbij worden voorgaande ids in de lijst toegevoegd
                        if (id == "1" || id == "2" || id == "4" || id == "5" || id == "8")
                        {
                            provincieIds.Add(id);
                        };

                    }
                }
            }
            // 1: lees provincienaam + provincieId + gemeenteId // index: 3
            using (var stream = new StreamReader(Path.Combine(_path, _extractPath, fileNames[3]))) // opent een bestand "ProvincieInfo.csv", // index: 3
            {
                string line;
                // Skip header
                stream.ReadLine();
                var timer = new Stopwatch();
                timer.Start();
                while ((line = stream.ReadLine()) != null)
                {
                    string[] values = line.Trim().Split(";");
                    int gemeenteId = int.Parse(values[0]);
                    int provincieId = int.Parse(values[1]);
                    if (values[2] == "nl" && values.Length == 4)
                    {
                        bool gemeenteGevonden = false;
                        foreach (var g in gemeenteProvincieLinks)
                        {
                            if (g.GemeenteId == gemeenteId)
                            {
                                gemeenteGevonden = true;
                                break;
                            }
                        }
                        if (!gemeenteGevonden)
                        {
                            gemeenteProvincieLinks.Add(new ProvincieGemeente(values[3]) { GemeenteId = gemeenteId, ProvincieId = provincieId });
                        }
                    }
                }
                timer.Stop();
                TimeSpan timeTaken = timer.Elapsed;
                System.Diagnostics.Debug.WriteLine("Time taken: " + timeTaken.ToString(@"m\:ss\.fff")); // inclusief milliseconden
            }

            // 2: gemeentename + gemeenteid: gebruik de gemeenteProvincieLinks structuur // index: 1
            using (StreamReader stream = new(Path.Combine(_path, _extractPath, fileNames[1]))) // opent een bestand "Gemeentenaam.csv", // index: 1
            {
                string line;
                stream.ReadLine(); // skip header
                while ((line = stream.ReadLine()) != null)
                {
                    string[] values = line.Trim().Split(';');
                    int gemeenteId = int.Parse(values[1]);
                    if (values[2] == "nl" && values.Length == 4)
                    {
                        bool gemeenteGevonden = false;
                        foreach (var g in gemeenteGemeenteLinks)
                        {
                            if (g.GemeenteId == gemeenteId)
                            {
                                gemeenteGevonden = true;
                                break;
                            }
                        }
                        if (!gemeenteGevonden)
                        {
                            gemeenteGemeenteLinks.Add(new ProvincieGemeente(values[3]) { GemeenteId = gemeenteId });// voegt toe aan de lijst.
                        }
                    }
                }
            }

            // 3: lees straatnaamid + gemeenteId // index: 2 
            using (StreamReader stream = new(Path.Combine(_path, _extractPath, fileNames[2]))) // opent een bestand "StraatnaamID_gemeenteID.csv", // index: 2
            {
                string line;
                stream.ReadLine(); // skip header
                while ((line = stream.ReadLine()) != null)
                {
                    string[] values = line.Trim().Split(';');
                    ProvincieGemeente gemeenteInfo = new() { GemeenteId = int.Parse(values[1]), StraatNaamId = int.Parse(values[0]) };

                    gemeenteLinks.Add(gemeenteInfo);
                }
            }

            // Voor performantie: HashSet + Dictionary + SortedSet
            // 4: lees straatnamen (heel groot bestand)
            using (StreamReader stream = new(Path.Combine(_path, _extractPath, fileNames[0]))) // opent een bestand "straatnamen.csv", // index: 0
            {
                string line;
                int teller = 0;
                stream.ReadLine(); // skip header
                while ((line = stream.ReadLine()) != null)
                {
                    string[] values = line.Trim().Split(';');
                    GemeenteStraat straatInfo = new() { StraatNaamId = int.Parse(values[0]), Straatnaam = values[1] };


                    if (straatInfo.StraatNaamId > 0 && straatInfo.Straatnaam != string.Empty)
                    {
                        straatLinks.Add(straatInfo);
                    }
                    /*  if (teller == values.Length)
                      {*/
                    //break;
                    //}
                    //teller++;

                }
            }
        }
        public void Unzip(string fileName)
        {
            var fileRef = Path.Combine(this._path, fileName);
            // Check if file exists
            if (!File.Exists(fileRef))
            {
                throw new FileNotFoundException("File not found", fileRef);
            }
            try
            {
                ZipFile.ExtractToDirectory(fileRef, Path.Combine(_path, this._extractPath));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        public void VoegToeAanDictionary()
        {
            totaleData = new();

            foreach (var provincie in gemeenteProvincieLinks)
            {
                if (provincie.ProvincieId == 1 || provincie.ProvincieId == 2 || provincie.ProvincieId == 4 || provincie.ProvincieId == 5 || provincie.ProvincieId == 8)
                {

                    if (!totaleData.ContainsKey(provincie.Provincie)) // als hij deze provincie nog niet heeft, voegt hij hem toe
                    {

                        totaleData.Add(provincie.Provincie, new Dictionary<string, SortedSet<string>>());
                    }
                    foreach (var gemeente in gemeenteGemeenteLinks)
                    {
                        if (gemeente.GemeenteId == provincie.GemeenteId) // als de gemeenteId van de provincie overeenkomt met de gemeenteId van de gemeente gaat hij verder
                        {
                            if (!totaleData[provincie.Provincie].ContainsKey(gemeente.Gemeente)) // als hij deze gemeente nog niet heeft, voegt hij hem toe
                            {
                                totaleData[provincie.Provincie].Add(gemeente.Gemeente, new SortedSet<string>());
                            }
                            foreach (var straat in gemeenteLinks)
                            {
                                if (straat.GemeenteId == gemeente.GemeenteId) // als de gemeenteId van de straat overeenkomt met de gemeenteId van de gemeente gaat hij verder
                                {
                                    foreach (var straatnaam in straatLinks)
                                    {
                                        if (straat.StraatNaamId == straatnaam.StraatNaamId) // als de straatnaamId van de straat overeenkomt met de straatnaamId van de straatnaam gaat hij verder
                                        {
                                            totaleData[provincie.Provincie][gemeente.Gemeente].Add(straatnaam.Straatnaam);
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }
        public void MaakMappenAan()
        {
            string resultPath = Path.Combine(_path, "Provincies");

            if (Directory.Exists(resultPath)) // als de map al bestaat, verwijder hem en maak hem opnieuw aan
            {
                Directory.Delete(resultPath, true);
                Directory.CreateDirectory(resultPath);
            }
            else
            {
                Directory.CreateDirectory(resultPath);
            }
            foreach (var provincie in totaleData)
            {
                string provincieMap = Path.Combine(resultPath, provincie.Key);
                Directory.CreateDirectory(provincieMap);
                foreach (var gemeente in provincie.Value)
                {
                    string gemeenteMap = Path.Combine(provincieMap, gemeente.Key);
                    Directory.CreateDirectory(gemeenteMap);

                    foreach (var straat in gemeente.Value)
                    {
                        using (StreamWriter sw = File.AppendText(Path.Combine(gemeenteMap, $"alleStraten.txt")))
                        {
                            sw.WriteLine(straat);
                        };
                    }
                }
            }
        }
        #endregion
    }
}