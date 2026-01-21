using Enrichify.Controllers;
using Enrichify.Models;
using Enrichify.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Text;
using Xunit;

namespace Enrichify.Tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly Mock<IHunterService> _mockHunterService;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _mockHunterService = new Mock<IHunterService>();
            _controller = new HomeController(_mockHunterService.Object);

            // Setup TempData
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
        }

        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Act
            var result = _controller.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            // Act
            var result = _controller.Privacy();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task UploadCsv_WithNullFile_ReturnsRedirectWithError()
        {
            // Act
            var result = await _controller.UploadCsv(null);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirect = result as RedirectToActionResult;
            redirect.ActionName.Should().Be("Index");
            _controller.TempData["ErrorMessage"].Should().Be("Please upload a valid CSV file.");
        }

        [Fact]
        public async Task UploadCsv_WithNonCsvFile_ReturnsRedirectWithError()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.txt");
            fileMock.Setup(f => f.Length).Returns(100);

            // Act
            var result = await _controller.UploadCsv(fileMock.Object);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            _controller.TempData["ErrorMessage"].Should().Be("Please upload a CSV file.");
        }

        [Fact]
        public async Task UploadCsv_WithEmptyFile_ReturnsRedirectWithError()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.csv");
            fileMock.Setup(f => f.Length).Returns(0);

            // Act
            var result = await _controller.UploadCsv(fileMock.Object);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            _controller.TempData["ErrorMessage"].Should().Be("Please upload a valid CSV file.");
        }

        [Fact]
        public async Task UploadCsv_WithValidCsv_RedirectsToPreview()
        {
            // Arrange
            var csvContent = "Name,Email,Company\nJohn Doe,john@example.com,Acme Corp\nJane Smith,jane@example.com,Tech Inc";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.csv");
            fileMock.Setup(f => f.Length).Returns(stream.Length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

            // Act
            var result = await _controller.UploadCsv(fileMock.Object);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirect = result as RedirectToActionResult;
            redirect.ActionName.Should().Be("Preview");
            _controller.TempData["ContactsJson"].Should().NotBeNull();
            _controller.TempData["FileName"].Should().Be("test.csv");
        }

        [Fact]
        public async Task UploadCsv_WithMoreThan5Contacts_ReturnsError()
        {
            // Arrange
            var csvContent = "Name,Email,Company\n" +
                           "John Doe,john@example.com,Acme Corp\n" +
                           "Jane Smith,jane@example.com,Tech Inc\n" +
                           "Bob Johnson,bob@example.com,Corp LLC\n" +
                           "Alice Brown,alice@example.com,Dev Co\n" +
                           "Charlie Wilson,charlie@example.com,Test Ltd\n" +
                           "David Lee,david@example.com,Extra Corp";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.csv");
            fileMock.Setup(f => f.Length).Returns(stream.Length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

            // Act
            var result = await _controller.UploadCsv(fileMock.Object);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            _controller.TempData["ErrorMessage"].ToString().Should()
                .StartWith("Please limit your CSV to 5 contacts or fewer")
                .And.Contain("6 contacts");
        }

        [Fact]
        public async Task UploadCsv_WithInvalidCsvFormat_ReturnsError()
        {
            // Arrange
            var csvContent = "Invalid,CSV,Format\nNo proper structure here";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.csv");
            fileMock.Setup(f => f.Length).Returns(stream.Length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

            // Act
            var result = await _controller.UploadCsv(fileMock.Object);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            _controller.TempData["ErrorMessage"].Should().NotBeNull();
        }

        [Fact]
        public void Preview_WithoutContactsData_RedirectsToIndex()
        {
            // Act
            var result = _controller.Preview();

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirect = result as RedirectToActionResult;
            redirect.ActionName.Should().Be("Index");
            _controller.TempData["ErrorMessage"].Should().Be("No data to preview. Please upload a CSV file first.");
        }

        [Fact]
        public void Preview_WithValidContactsData_ReturnsViewWithContacts()
        {
            // Arrange
            var contacts = new List<Contact>
            {
                new Contact { Name = "John Doe", Email = "john@example.com", Company = "Acme Corp" }
            };
            var contactsJson = System.Text.Json.JsonSerializer.Serialize(contacts);
            _controller.TempData["ContactsJson"] = contactsJson;
            _controller.TempData["FileName"] = "test.csv";

            // Act
            var result = _controller.Preview();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeOfType<List<Contact>>();
            var model = viewResult.Model as List<Contact>;
            model.Should().HaveCount(1);
            model[0].Name.Should().Be("John Doe");
        }

        [Fact]
        public async Task EnrichFromPreview_WithoutContactsData_RedirectsToIndex()
        {
            // Act
            var result = await _controller.EnrichFromPreview();

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            _controller.TempData["ErrorMessage"].Should().Be("Session expired. Please upload your CSV again.");
        }

        [Fact]
        public async Task EnrichFromPreview_WithValidData_EnrichesContactsAndReturnsResults()
        {
            // Arrange
            var contacts = new List<Contact>
            {
                new Contact { Name = "John Doe", Email = "", Company = "example.com" }
            };
            var contactsJson = System.Text.Json.JsonSerializer.Serialize(contacts);
            _controller.TempData["ContactsJson"] = contactsJson;

            _mockHunterService
                .Setup(s => s.FindEmail("example.com", "John", "Doe"))
                .ReturnsAsync("john.doe@example.com");

            // Act
            var result = await _controller.EnrichFromPreview();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Results");
            viewResult.Model.Should().BeOfType<List<Contact>>();
            var model = viewResult.Model as List<Contact>;
            model[0].EnrichedEmail.Should().Be("john.doe@example.com");
        }

        [Fact]
        public async Task EnrichFromPreview_WithInvalidContactData_SetsInvalidDataMessage()
        {
            // Arrange
            var contacts = new List<Contact>
            {
                new Contact { Name = "", Email = "", Company = "" }
            };
            var contactsJson = System.Text.Json.JsonSerializer.Serialize(contacts);
            _controller.TempData["ContactsJson"] = contactsJson;

            // Act
            var result = await _controller.EnrichFromPreview();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            var model = viewResult.Model as List<Contact>;
            model[0].EnrichedEmail.Should().Be("Invalid data");
        }

        [Fact]
        public async Task EnrichFromPreview_WithSingleNameContact_UsesFirstNameOnly()
        {
            // Arrange
            var contacts = new List<Contact>
            {
                new Contact { Name = "Madonna", Email = "", Company = "music.com" }
            };
            var contactsJson = System.Text.Json.JsonSerializer.Serialize(contacts);
            _controller.TempData["ContactsJson"] = contactsJson;

            _mockHunterService
                .Setup(s => s.FindEmail("music.com", "Madonna", ""))
                .ReturnsAsync("madonna@music.com");

            // Act
            var result = await _controller.EnrichFromPreview();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var model = (result as ViewResult).Model as List<Contact>;
            model[0].EnrichedEmail.Should().Be("madonna@music.com");
        }
    }
}