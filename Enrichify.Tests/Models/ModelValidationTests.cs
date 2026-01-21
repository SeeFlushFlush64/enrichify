using Enrichify.ViewModels;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Enrichify.Tests.Models
{
    public class ModelValidationTests
    {
        private IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }

        #region RegisterViewModel Tests

        [Fact]
        public void RegisterViewModel_WithValidData_PassesValidation()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "Test123!",
                ConfirmPassword = "Test123!"
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public void RegisterViewModel_WithEmptyFullName_FailsValidation()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                FullName = "",
                Email = "john@example.com",
                Password = "Test123!",
                ConfirmPassword = "Test123!"
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("FullName"));
        }

        [Fact]
        public void RegisterViewModel_WithInvalidEmail_FailsValidation()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                FullName = "John Doe",
                Email = "invalid-email",
                Password = "Test123!",
                ConfirmPassword = "Test123!"
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void RegisterViewModel_WithShortPassword_FailsValidation()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "Test1",
                ConfirmPassword = "Test1"
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("Password"));
        }

        [Fact]
        public void RegisterViewModel_WithMismatchedPasswords_FailsValidation()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "Test123!",
                ConfirmPassword = "Different123!"
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("ConfirmPassword"));
        }

        [Fact]
        public void RegisterViewModel_WithEmptyPassword_FailsValidation()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "",
                ConfirmPassword = ""
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("Password"));
        }

        #endregion

        #region LoginViewModel Tests

        [Fact]
        public void LoginViewModel_WithValidData_PassesValidation()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "john@example.com",
                Password = "Test123!",
                RememberMe = false
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public void LoginViewModel_WithEmptyEmail_FailsValidation()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "",
                Password = "Test123!",
                RememberMe = false
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void LoginViewModel_WithInvalidEmail_FailsValidation()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "not-an-email",
                Password = "Test123!",
                RememberMe = false
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void LoginViewModel_WithEmptyPassword_FailsValidation()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "john@example.com",
                Password = "",
                RememberMe = false
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("Password"));
        }

        [Fact]
        public void LoginViewModel_RememberMe_DefaultsToFalse()
        {
            // Arrange & Act
            var model = new LoginViewModel
            {
                Email = "john@example.com",
                Password = "Test123!"
            };

            // Assert
            model.RememberMe.Should().BeFalse();
        }

        #endregion
    }
}