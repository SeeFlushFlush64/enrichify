using CsvHelper;
using Enrichify.Models;
using Enrichify.Services; // your HunterService
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Enrichify.Controllers
{
    //cant login to ftp deploy
    [Authorize]
    public class HomeController : Controller
    {
        private readonly HunterService _hunterService;

        public HomeController(HunterService hunterService)
        {
            _hunterService = hunterService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return View("Index");

            List<Contact> contacts;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null
                };

                using (var csv = new CsvReader(reader, config))
                {
                    contacts = csv.GetRecords<Contact>().ToList();
                }
            }

            // Enrich each contact asynchronously
            foreach (var contact in contacts)
            {
                // Split first and last name
                var names = contact.Name.Split(' ');
                string firstName = names[0];
                string lastName = names.Length > 1 ? names[1] : "";
                contact.EnrichedEmail = await _hunterService.FindEmail(contact.Company, firstName, lastName);
            }

            // Pass the enriched list directly to the Results view
            return View("Results", contacts);
        }
    }
}
