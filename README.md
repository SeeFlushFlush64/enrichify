# Enrichify

> A free, production-ready email enrichment platform that finds and verifies professional email addresses without breaking the bank.

[![Live Demo](https://img.shields.io/badge/demo-live-success)](https://enrichify-eph8f2bwh2fwb3a0.southeastasia-01.azurewebsites.net/)
[![Build Status](https://github.com/SeeFlushFlush64/enrichify/workflows/CI%20-%20Build%20and%20Test/badge.svg)](https://github.com/SeeFlushFlush64/enrichify/actions)
[![Azure](https://img.shields.io/badge/Azure-Deployed-0078D4)](https://enrichify-eph8f2bwh2fwb3a0.southeastasia-01.azurewebsites.net/)

## 🎯 What is Enrichify?

Enrichify is a web-based email enrichment application powered by **Hunter.io API** that helps sales and marketing teams find verified email addresses for their leads. Simply create an account, upload a CSV with names and companies, and get back verified email addresses—completely free.

**[Try it live →](https://enrichify-eph8f2bwh2fwb3a0.southeastasia-01.azurewebsites.net/)**

### Key Features

- **🔐 User Authentication** - Secure registration and login system with password hashing
- **📊 Bulk CSV Processing** - Upload up to 5 contacts at once with name and company data
- **✅ Email Verification** - 95%+ accuracy powered by Hunter.io's extensive database
- **⚡ Instant Results** - Get enriched data in seconds with real-time processing
- **📥 Export Ready** - Download results as CSV for immediate use in campaigns
- **🔒 Secure & Private** - Your data is processed securely on Azure infrastructure
- **💯 100% Free Forever** - No credit card required, no hidden fees

## 🚀 Live Demo

**Production URL:** https://enrichify-eph8f2bwh2fwb3a0.southeastasia-01.azurewebsites.net/

### How It Works

1. **Create or Sign into your Account** - Sign up with your email and create a secure password
<img width="1519" height="784" alt="image" src="https://github.com/user-attachments/assets/2d50fdd2-4931-45ef-aa63-8e384dd94eca" />
<img width="1512" height="786" alt="image" src="https://github.com/user-attachments/assets/083297df-8eb5-43b7-96f1-cb061b746d1e" />
3. **Upload Your CSV** - Drag and drop a file with Name, Company, and Email columns
<img width="1512" height="786" alt="image" src="https://github.com/user-attachments/assets/1dd0ff4b-7559-4565-96aa-2d74c1c0896f" />
5. **Preview your Uploaded CSV** - We require Name, Company and Email columns to do the email enrichment
<img width="1499" height="812" alt="image" src="https://github.com/user-attachments/assets/7b5bdbc2-2ea4-4b30-9a61-430c7bd75f6b" />
7. **We Find & Verify** - Our system searches Hunter.io's database to find verified emails
<img width="1503" height="812" alt="image" src="https://github.com/user-attachments/assets/e001ed8c-8a8f-4df9-a3a1-7206f1bc0258" />
8. **Download Results** - Get your enriched CSV instantly with verified email addresses

## 🛠️ Tech Stack

### Backend
- **ASP.NET Core 8.0 MVC** - Modern web framework with Razor views
- **ASP.NET Core Identity** - Secure authentication and authorization system
- **C#** - Primary programming language
- **Entity Framework Core** - ORM for database operations
- **Hunter.io API** - Email enrichment and verification service
- **RESTful API Integration** - Clean API client implementation

### Database
- **Azure SQL Database** - Cloud-hosted relational database
- **SQL Server (SSMS)** - Local development database

### Frontend
- **Bootstrap 5** - Responsive UI framework
- **jQuery** - DOM manipulation and AJAX
- **HTML5/CSS3** - Modern web standards

### DevOps & Infrastructure
- **Azure App Service** - Hosting platform
- **GitHub Actions** - CI/CD automation
- **Azure Resource Manager** - Infrastructure management

## 📦 Project Structure

```
Enrichify/
├── Controllers/          # MVC controllers
├── Models/              # Data models and view models
├── Services/            # Business logic and API integration
├── Data/                # Database context and migrations
├── Views/               # Razor view templates
├── wwwroot/             # Static assets (CSS, JS, images)
├── Migrations/          # EF Core database migrations
└── appsettings.json     # Configuration (excluded from repo)

Enrichify.Tests/
├── Controllers/         # Controller unit tests
├── Services/           # Service layer tests
└── UnitTest1.cs        # Core functionality tests
```

## 🔧 Local Development Setup

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or SQL Server Express
- [Hunter.io API Key](https://hunter.io/api) (free tier available)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/YOUR_USERNAME/enrichify.git
   cd enrichify
   ```

2. **Configure application settings**
   
   Create `appsettings.json` in the `Enrichify` folder:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EnrichifyDB;Trusted_Connection=True;"
     },
     "Hunter": {
       "ApiKey": "YOUR_HUNTER_API_KEY_HERE"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*"
   }
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Apply database migrations**
   ```bash
   cd Enrichify
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access the app**
   
   Navigate to `https://localhost:5001` or `http://localhost:5000`

## 🧪 Testing

This project includes comprehensive unit tests using xUnit.

### Run all tests
```bash
dotnet test
```

### Run tests with detailed output
```bash
dotnet test --verbosity detailed
```

### Generate test coverage report
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 🚢 Deployment

The application is automatically deployed to Azure App Service using GitHub Actions when changes are pushed to the `main` branch.

### CI/CD Pipeline

- **Continuous Integration (`ci.yml`)** - Runs on every push and pull request
  - Restores dependencies
  - Builds the project
  - Runs all unit tests
  - Validates code quality

- **Continuous Deployment (`azure-deploy.yml`)** - Deploys to Azure on main branch pushes
  - Builds release configuration
  - Injects production secrets
  - Publishes to Azure App Service
  - Automatic rollback on failure

### Manual Deployment

To deploy manually using Azure CLI:

```bash
# Login to Azure
az login

# Build and publish
dotnet publish -c Release -o ./publish

# Deploy to Azure
az webapp deployment source config-zip \
  --resource-group YOUR_RESOURCE_GROUP \
  --name enrichify \
  --src ./publish.zip
```

## 🔐 Environment Variables

Configure these secrets in GitHub Actions or Azure App Service:

| Variable | Description | Required |
|----------|-------------|----------|
| `CONNECTION_STRING_PRODUCTION` | Azure SQL Database connection string | ✅ |
| `HUNTER_API_KEY` | Hunter.io API key for email enrichment | ✅ |
| `AZURE_CREDENTIALS` | Azure service principal credentials | ✅ |
| `AZURE_RESOURCE_GROUP` | Azure resource group name | ✅ |
| `AZURE_WEBAPP_NAME` | Azure web app name | ✅ |

## 📝 API Integration

Enrichify integrates with **Hunter.io's Email Finder API** to discover and verify professional email addresses.

### Why Hunter.io?
- **260+ million verified email addresses** in their database
- **95%+ accuracy rate** for email verification
- **Domain-wide email patterns** detection
- **Confidence score** for each found email
- **Free tier available** for development and testing

### Rate Limiting
- Free tier: 50 requests/month
- Implements retry logic with exponential backoff
- Graceful degradation when limits are reached
- Clear user feedback on API status

### Error Handling
- Validates API responses for accuracy
- Handles network failures gracefully
- Provides user-friendly error messages
- Comprehensive logging for debugging
- Automatic retry on transient failures

## 🏗️ Architecture Highlights

- **MVC Pattern** - Separation of concerns with Models, Views, and Controllers
- **ASP.NET Core Identity** - Built-in authentication with password hashing and user management
- **Repository Pattern** - Abstracted data access layer
- **Dependency Injection** - Loosely coupled services
- **Async/Await** - Non-blocking I/O operations
- **Entity Framework Core** - Code-first database migrations
- **Responsive Design** - Mobile-first UI approach
- **Session Management** - Secure user session handling with cookies

## 📊 Quality Assurance

Quality assurance documentation is maintained in [`QA-NOTES.md`](QA-NOTES.md), covering:

- ✅ Functional testing of enrichment workflows
- ✅ API response validation and error scenarios
- ✅ Rate limiting and retry logic verification
- ✅ Database operations and data integrity
- ✅ UI/UX testing across devices
- ✅ Regression testing after feature updates

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [Hunter.io](https://hunter.io/) - Powering our email enrichment with their comprehensive API
- [Microsoft ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity) - For secure authentication
- [Bootstrap](https://getbootstrap.com/) - For the responsive UI framework
- [Azure](https://azure.microsoft.com/) - For reliable cloud hosting infrastructure

## 📧 Contact

Michael Rhey Palaganas - michaelrheypalaganas71@gmail.com

Project Link: [https://github.com/YOUR_USERNAME/enrichify](https://github.com/YOUR_USERNAME/enrichify)

---

⭐ **Star this repo if you find it helpful!**
