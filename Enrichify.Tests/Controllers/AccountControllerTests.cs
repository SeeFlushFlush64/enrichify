using Enrichify.Controllers;
using Enrichify.Models;
using Enrichify.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;

namespace Enrichify.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _mockUserManager = MockUserManager();
            _mockSignInManager = MockSignInManager(_mockUserManager);
            _controller = new AccountController(_mockUserManager.Object, _mockSignInManager.Object);

            // Setup TempData
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
        }

        private Mock<UserManager<ApplicationUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        private Mock<SignInManager<ApplicationUser>> MockSignInManager(
            Mock<UserManager<ApplicationUser>> userManager)
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

            return new Mock<SignInManager<ApplicationUser>>(
                userManager.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                null, null, null, null);
        }

        [Fact]
        public void Register_Get_ReturnsViewResult()
        {
            // Act
            var result = _controller.Register();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Register_Post_WithValidModel_CreatesUserAndRedirects()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Password = "Test123!",
                ConfirmPassword = "Test123!",
                FullName = "Test User"
            };

            _mockUserManager
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockSignInManager
                .Setup(x => x.SignInAsync(It.IsAny<ApplicationUser>(), false, null))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Register(model);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirect = result as RedirectToActionResult;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
            _controller.TempData["WelcomeMessage"].Should().NotBeNull();
        }

        [Fact]
        public async Task Register_Post_WithInvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Password = "Test123!",
                ConfirmPassword = "Different123!",
                FullName = "Test User"
            };

            _controller.ModelState.AddModelError("ConfirmPassword", "Passwords don't match.");

            // Act
            var result = await _controller.Register(model);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().Be(model);
        }

        [Fact]
        public async Task Register_Post_WhenUserCreationFails_AddsModelErrors()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Password = "Test123!",
                ConfirmPassword = "Test123!",
                FullName = "Test User"
            };

            var errors = new[]
            {
                new IdentityError { Description = "Email already exists" }
            };

            _mockUserManager
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(errors));

            // Act
            var result = await _controller.Register(model);

            // Assert
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.Should().ContainKey(string.Empty);
            _controller.ModelState[string.Empty].Errors[0].ErrorMessage.Should().Be("Email already exists");
        }

        [Fact]
        public async Task Register_Post_CreatesUserWithCorrectProperties()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Password = "Test123!",
                ConfirmPassword = "Test123!",
                FullName = "John Doe"
            };

            ApplicationUser capturedUser = null;
            _mockUserManager
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .Callback<ApplicationUser, string>((user, pwd) => capturedUser = user)
                .ReturnsAsync(IdentityResult.Success);

            _mockSignInManager
                .Setup(x => x.SignInAsync(It.IsAny<ApplicationUser>(), false, null))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.Register(model);

            // Assert
            capturedUser.Should().NotBeNull();
            capturedUser.Email.Should().Be("test@example.com");
            capturedUser.UserName.Should().Be("test@example.com");
            capturedUser.FullName.Should().Be("John Doe");
        }

        [Fact]
        public void Login_Get_ReturnsViewResult()
        {
            // Act
            var result = _controller.Login();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Login_Post_WithValidCredentials_RedirectsToHome()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "Test123!",
                RememberMe = false
            };

            _mockSignInManager
                .Setup(x => x.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Act
            var result = await _controller.Login(model);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirect = result as RedirectToActionResult;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
        }

        [Fact]
        public async Task Login_Post_WithInvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "WrongPassword!",
                RememberMe = false
            };

            _mockSignInManager
                .Setup(x => x.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act
            var result = await _controller.Login(model);

            // Assert
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.Should().ContainKey(string.Empty);
            _controller.ModelState[string.Empty].Errors[0].ErrorMessage.Should().Be("Invalid login attempt.");
        }

        [Fact]
        public async Task Login_Post_WithInvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "invalid-email",
                Password = "Test123!",
                RememberMe = false
            };

            _controller.ModelState.AddModelError("Email", "Invalid email format");

            // Act
            var result = await _controller.Login(model);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().Be(model);
        }

        [Fact]
        public async Task Login_Post_WithRememberMe_PassesCorrectParameter()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "Test123!",
                RememberMe = true
            };

            _mockSignInManager
                .Setup(x => x.PasswordSignInAsync(model.Email, model.Password, true, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Act
            await _controller.Login(model);

            // Assert
            _mockSignInManager.Verify(
                x => x.PasswordSignInAsync(model.Email, model.Password, true, false),
                Times.Once);
        }

        [Fact]
        public async Task Logout_SignsOutUserAndRedirectsToHome()
        {
            // Arrange
            _mockSignInManager
                .Setup(x => x.SignOutAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Logout();

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirect = result as RedirectToActionResult;
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
            _mockSignInManager.Verify(x => x.SignOutAsync(), Times.Once);
        }
    }
}