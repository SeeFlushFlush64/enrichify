using CsvHelper;
using Enrichify.Models;
using Enrichify.Services;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;

namespace Enrichify.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHunterService _hunterService;

        public HomeController(IHunterService hunterService)
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

                if (contacts == null || contacts.Count == 0)
                {
                    TempData["ErrorMessage"] = "The CSV file is empty or contains no valid contacts.";
                    return RedirectToAction("Index");
                }

                if (contacts.Count > 5)
                {
                    TempData["ErrorMessage"] = $"Please limit your CSV to 5 contacts or fewer. Your file contains {contacts.Count} contacts.";
                    return RedirectToAction("Index");
                }

                // Store in TempData as JSON string
                TempData["ContactsJson"] = JsonSerializer.Serialize(contacts);
                TempData["FileName"] = file.FileName;

                return RedirectToAction("Preview");
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

        [HttpGet]
        public IActionResult Preview()
        {
            var contactsJson = TempData["ContactsJson"] as string;
            var fileName = TempData["FileName"] as string;

            if (string.IsNullOrEmpty(contactsJson))
            {
                TempData["ErrorMessage"] = "No data to preview. Please upload a CSV file first.";
                return RedirectToAction("Index");
            }

            try
            {
                var contacts = JsonSerializer.Deserialize<List<Contact>>(contactsJson);

                // Keep data for the form post
                TempData.Keep("ContactsJson");
                TempData.Keep("FileName");

                ViewBag.FileName = fileName ?? "Unknown File";
                return View(contacts);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading preview: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EnrichFromPreview()
        {
            var contactsJson = TempData["ContactsJson"] as string;

            if (string.IsNullOrEmpty(contactsJson))
            {
                TempData["ErrorMessage"] = "Session expired. Please upload your CSV again.";
                return RedirectToAction("Index");
            }

            try
            {
                var contacts = JsonSerializer.Deserialize<List<Contact>>(contactsJson);

                // Enrich each contact
                foreach (var contact in contacts)
                {
                    if (string.IsNullOrWhiteSpace(contact.Name) || string.IsNullOrWhiteSpace(contact.Company))
                    {
                        contact.EnrichedEmail = "Invalid data";
                        continue;
                    }

                    var names = contact.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    string firstName = names[0];
                    string lastName = names.Length > 1 ? names[^1] : "";

                    contact.EnrichedEmail = await _hunterService.FindEmail(contact.Company, firstName, lastName);
                }

                return View("Results", contacts);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error during enrichment: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}