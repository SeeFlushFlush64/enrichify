using CsvHelper;
using Enrichify.Models;
using Enrichify.Services; // your HunterService
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Enrichify.Controllers
{
    //last try, web deploy

    public class HomeController : Controller
    {
        private readonly HunterService _hunterService;

        public HomeController(HunterService hunterService)
        {
            _hunterService = hunterService;
        }

        public IActionResult Privacy() 
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please upload a valid CSV file.";
                return RedirectToAction("Index");
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Please upload a CSV file.";
                return RedirectToAction("Index");
            }

            try
            {
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

                // Check if CSV is empty
                if (contacts == null || contacts.Count == 0)
                {
                    TempData["ErrorMessage"] = "The CSV file is empty or contains no valid contacts.";
                    return RedirectToAction("Index");
                }

                // Limit to 5 contacts per upload
                if (contacts.Count > 5)
                {
                    TempData["ErrorMessage"] = $"Please limit your CSV to 5 contacts or fewer. Your file contains {contacts.Count} contacts.";
                    return RedirectToAction("Index");
                }

                // Enrich each contact asynchronously
                foreach (var contact in contacts)
                {
                    // Skip if contact data is incomplete
                    if (string.IsNullOrWhiteSpace(contact.Name) || string.IsNullOrWhiteSpace(contact.Company))
                    {
                        contact.EnrichedEmail = "Invalid data";
                        continue;
                    }

                    // Split first and last name
                    var names = contact.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    string firstName = names[0];
                    string lastName = names.Length > 1 ? names[^1] : ""; // Use last element as last name

                    contact.EnrichedEmail = await _hunterService.FindEmail(contact.Company, firstName, lastName);
                }

                // Pass the enriched list directly to the Results view
                return View("Results", contacts);
            }
            catch (CsvHelperException ex)
            {
                TempData["ErrorMessage"] = "Invalid CSV format. Please ensure your CSV has Name, Company, and Email columns.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error processing file: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }

}
