# QA Notes – Enrichify Web Application

## Scope of Testing

- Web UI (ASP.NET MVC)
- REST API integrations (Hunter.io)
- Database persistence (SQL Server)

## Manual Test Areas Covered

### Functional Testing

- Verified email enrichment flow with valid and invalid inputs
- Tested API error handling (rate limits, invalid API keys)
- Validated correct data storage in database

### API Testing

- Verified REST API responses using expected HTTP status codes
- Validated JSON response structure and required fields
- Tested failure scenarios and edge cases

### Regression Testing

- Re-tested core features after code changes
- Verified no breaking changes after updates

### UI & Responsiveness

- Checked layout behavior across desktop and mobile viewports
- Validated form validation and error messages

## Tools Used

- Browser DevTools
- Manual API testing
- SQL Server queries
