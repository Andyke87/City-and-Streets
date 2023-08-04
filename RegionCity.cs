namespace StratenAanzet.Domain
{
    public class ProvincieGemeente
    {
        private string v;
        private int n;

        #region Properties
        public string Gemeente { get; set; }
        public string Provincie { get; set; }
        public int GemeenteId { get; set; }
        public int ProvincieId { get; set; }
        public string? Straatnaam { get; set; }
        public int StraatNaamId { get; set; }
        #endregion

        public ProvincieGemeente(string v) : this()
        {
            Straatnaam= v;
            Gemeente= v;
            Provincie = v;
            this.v = v;
        }
        public ProvincieGemeente() 
        {
            StraatNaamId = n;
            ProvincieId = n;
            this.n = n;

        }
    }
}