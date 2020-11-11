# Local Deployment
To locally deploy the web application (either for local development or local hosting):
1. Clone the repo
2. Copy the configuration from the repo README to your secrets.json or appsettings.json. You will need to define the following:
    1. SQL Database connection string: local or remote database
    2. Cosmos DB connection information: Azure Cosmos Db deployment or local emulator
    3. Enable one of the available authentication mechanisms and create a SendGrid account (see Post Deployment below). Please note that Azure Key Vault integration is built in to Azure App Service. For local purposes, please add the values to either secrets.json or appsettings.json.

# Azure Deployment
To deploy the Azure Services required for the web application, please click the below button

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fistavrinides%2FAzureChallenge%2Fmaster%2FDeploy%2Fazchallenge.json)

This will deploy the following resources:
- Azure SQL Server with an Azure SQL Database named AzureChallengeDb. This database holds the Identity Framework backend.
- Azure Key Vault to hold the secrets used
- Azure Web App (Windows) that hosts the ASP.NET Core web application
- Azure App Insights instance
- Azure Cosmos Db that serves as the web application database
- Azure SignalR Service. Used in Challenge analytics to get a live view of how many users have completed each question (aggregated)

# Post Deployment
After successfully deploying the template, you will need to manually configure the following services and follow the below steps:
1. [Deploy a SendGrid Account](https://docs.microsoft.com/en-us/azure/sendgrid-dotnet-how-to-send-email)
2. Once deployed (following the documentation), please retrieve your API Key. You will use the API Key value to set the Azure Web App appSetting called `SendGrid_Api_Key`. It is recommended to use the deployed Azure Key Vault to store the secret and [reference the value from Azure Key Vault](https://docs.microsoft.com/en-us/azure/app-service/app-service-key-vault-references). The Azure Web App already has delegated access to the Azure Key Vault secrets. You will need though to create an Azure Key Vault policy that includes your account as the Principal, with the correct Secret permission.
3. You will need to setup authentication. Using [this guide](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-v2-aspnet-core-webapp?view=aspnetcore-3.1), please create an App Registration. The supported account types option should be `Accounts in any organization directory (Any Azure AD directory - Multitenant)` if you want only Azure AD authentication (you can restrict the allowed domains - see below) or `Accounts in any organizational directory (Any Azure AD directory - Multitenant) and personal Microsoft accounts (e.g. Skype, Xbox)` to also allow Microsoft social identities.
   1. You will need to define a redirect uri (following this format: `https://<webappname>.azurewebsites.net/signin-microsoft` - where `<webappname>` is the name you selected from your Azure Web App)
   2. You will need to define a secret. The secret needs to be set in the Azure Web App appSetting called `Authentication:AzureAD:ClientSecret` - it is a best practice to use Azure KeyVault backed-secrets for such items.
   3. You will need to copy the App Registration's App (client) ID and set it in the Azure Web App appSetting called `Authentication:AzureAD:ClientId` - it is a best practice to use Azure KeyVault backed-secrets for such items.
   4. Since this is a multi-tenant application, you can restrict the allowed domains by setting a comma separate list of allowed domains. To do so, as an example, in the Azure Web App Setting called `Authentication:AzureAD:AllowedDomains` set the value to "mydomain.com,myotherdomain.com". This will allow only users from the mydomain.com and myotherdomain.com domain to login.
   4. Should you wish to enable [Facebook authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/facebook-logins?view=aspnetcore-3.1) or [Google authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-3.1), the web application is already setup and supports these. Follow the guides linked and the add the following (replace `<webappname>` with the name you have selected for your Azure Web App):
      1. For Facebook authentication
         1. Set the `Authentication:Facebook:Enabled` appSetting in the Azure Web App to `true`.
         2. The redirect uri will be `https://<webappname>.azurewebsites.net/signin-facebook`.
         3. You will need to set the Azure Web App appSettings called `Authentication:Facebook:AppId` and `Authentication:Facebook:AppSecret` with the respective values obtain from adding a Facebook application.
      2. For Google authentication
         1. Set the `Authentication:Google:Enabled` appSetting in the Azure Web App to `true`. 
         2. The redirect uri will be `https://<webappname>.azurewebsites.net/signin-google`.
         3. You will need to set the Azure Web App appSettings called `Authentication:Google:ClientId` and `Authentication:Google:ClientSecret` with the respective values obtain from adding Google API.