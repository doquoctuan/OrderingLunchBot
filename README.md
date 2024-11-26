# Ordering Lunch Bot 

The chatbot is designed to facilitate the management and ordering of meals by utilizing Google Sheets as its primary tool. It allows users to easily input their meal preferences, track orders, and manage inventory. Through an interactive interface, users can select from a variety of meal options, specify quantities, and place their orders directly. The chatbot also provides real-time updates on order status and availability, ensuring a seamless experience for both customers and meal service providers. This integration with Google Sheets allows for efficient data handling, making it easier to analyze trends, manage schedules, and optimize meal offerings based on customer preferences and feedback.

## Features

- Telegram Bot Integration: The bot uses the Telegram Bot API to interact with users, allowing them to place orders, cancel orders, and receive updates.
- Google Sheets Integration: The bot interacts with Google Sheets to manage and update order data.
- Order Management: Users can place orders, cancel orders, and confirm payments through the bot.
- Automated Tasks: The bot includes automated tasks for sending daily menus, reminders for unpaid orders, and blocking order tickets after a certain time.
- Unit Testing: The project includes unit tests to ensure the functionality of the order services.

## Tech Stack

- **Programming Language**: C#
- **Framework**: .NET 8.0
- **Cloud Platform**: Azure Functions
- **Database**: Google Sheets (via Google Sheets API)
- **Messaging Platform**: Telegram Bot API
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **HTTP Client**: Refit
- **Logging**: Microsoft.Extensions.Logging
- **Image Processing**: SixLabors.ImageSharp
- **Unit Testing**: xUnit
- **CI/CD**: GitHub Actions
- **Configuration Management**: Azure App Configuration, local.settings.json
- **Version Control**: Git
- **Package Management**: NuGet, npm (for dev dependencies like commitlint)
- **Infrastructure as Code**: Terraform

## SonarCloud

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=doquoctuan_OrderingLunchBot&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=doquoctuan_OrderingLunchBot) [![Bugs](https://sonarcloud.io/api/project_badges/measure?project=doquoctuan_OrderingLunchBot&metric=bugs)](https://sonarcloud.io/summary/new_code?id=doquoctuan_OrderingLunchBot) [![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=doquoctuan_OrderingLunchBot&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=doquoctuan_OrderingLunchBot) [![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=doquoctuan_OrderingLunchBot&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=doquoctuan_OrderingLunchBot) [![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=doquoctuan_OrderingLunchBot&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=doquoctuan_OrderingLunchBot) [![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=doquoctuan_OrderingLunchBot&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=doquoctuan_OrderingLunchBot) [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=doquoctuan_OrderingLunchBot&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=doquoctuan_OrderingLunchBot)

## High-level Architecture

The following diagram illustrates the high-level architecture of the **Ordering Lunch Bot**, showcasing the integration between the Telegram Bot API, Google Sheets, and the backend services. This architecture ensures seamless communication and efficient data management, providing a robust solution for meal ordering and management.

![High-level Architecture](https://i.imgur.com/2CLpTr6.png)

## Image
The image below showcases the interactive interface of the Ordering Lunch Bot. It displays a chat conversation where the bot presents meal options to the user, allowing them to select their preferred meal, specify the quantity, and place the order. The interface is user-friendly and designed to streamline the meal ordering process, providing a seamless experience for users.

![Poster Chatbot](https://i.imgur.com/zAYr2hb.jpg)

## Project Structure

```
.config/
    dotnet-tools.json
.github/
    workflows/
.gitignore
.husky/
    _/
    pre-commit
    task-runner.json
.vs/
OrderLunchBot/
OrderRiceBot/
ProjectEvaluation/
sd/
.vscode/
    extensions.json
    launch.json
    settings.json
    tasks.json
commitlint.config.js
OrderLunchBot.sln
package.json
README.md
src/
    ApiClients/
    AuthTokenHandler.cs
    bin/
    Constants.cs
    Entities/
    Exceptions/
    Extentions/
    Functions/
    GoogleSheetModels/
    Helper/
    host.json
    Interfaces/
    local.settings.json
    ...
terraform/
    ...
tests/
```

## Configuration

The project uses a local.settings.json file for configuration. Here is an example:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "TELEGRAM_BOT_TOKEN": "{{TELEGRAM_BOT_TOKEN}}",
    "TELEGRAM_BOT_SECRET": "{{TELEGRAM_BOT_SECRET}}",
    "GITHUB_TOKEN": "{{GITHUB_TOKEN}}",
    "GITHUB_REPOSITORY_URL": "https://api.github.com",
    "GITHUB_REPOSITORY_NAME": "{{GITHUB_REPOSITORY_NAME}}",
    "BASE_IMAGE": "{{BASE_IMAGE_URL}}",
    "SpreadSheetId": "{{SPREAD_SHEETID}}",
    "Google_ClientId": "{{GOOGLE_CLIENT_ID}}",
    "Google_ClientSecret": "{{GOOGLE_CLIENT_SECRET}}",
    "Google_TokenEndpoint": "{{GOOGLE_TOKEN_ENDPOINT}}",
    "Google_RefreshToken": "{{GOOGLE_REFRESHTOKEN}}"
  }
}
```

### How to Build and Run

1. Clone the repository

```
git clone https://github.com/your-repo/ordering-lunch-bot.git
cd ordering-lunch-bot
```

2. Install dependencies

```
npm install
```

3. Build the project

```
dotnet build
```

4. Run the project

```
dotnet run
```

5. Run tests

```
dotnet test
```

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## License

This project is licensed under the MIT License. See the LICENSE file for details.



