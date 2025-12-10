namespace Enrichify.Models
{
    public class Contact
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Company { get; set; }
        public string EnrichedEmail { get; set; } // Filled by Hunter.io
        public string EnrichedDomain { get; set; } // Optional enrichment
    }

}
