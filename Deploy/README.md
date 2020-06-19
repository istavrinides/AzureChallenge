# Deployment
To deploy the Azure Services required for the web application, please click the below button

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fistavrinides%2FAzureChallenge%2Fmaster%2FDeploy%2Fazchallenge.json)

This will deploy the following resources:
- Azure SQL Server with an Azure SQL Database named AzureChallengeDb. This database holds the Identity Framework backend.
- Azure Key Vault to hold the secrets used
- Azure Web App (Windows) that hosts the ASP.NET Core web application
- Azure App Insights instance
- Azure Cosmos Db that serves as the web application database
- Azure SignalR Service. Used in Challenge analytics to get a live view of how many users have completed each question (aggregated)

After successfully deploying the template, you will need to manually configure the following services and follow the below steps:
1. [Deploy a SendGrid Account](https://docs.microsoft.com/en-us/azure/sendgrid-dotnet-how-to-send-email)
2. Once deployed (following the documentation), please retrieve your API Key. You will use the API Key value to set the Azure Web App appSetting called `SendGrid_Api_Key`. It is recommended to use the deployed Azure Key Vault to store the secret and [reference the value from Azure Key Vault](https://docs.microsoft.com/en-us/azure/app-service/app-service-key-vault-references). The Azure Web App already has delegated access to the Azure Key Vault secrets.
3. You will need to setup authentication. Using [this guide](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-v2-aspnet-core-webapp?view=aspnetcore-3.1), please create an App Registration. 
   1. You will need to define a redirect uri (following this format: `https://\<webappname\>.azurewebsites.net/signin-microsoft`)
   2. You will need to define a secret. The secret needs to be set in the Azure Web App appSetting called `Authentication:Microsoft:ClientSecret`
   3. You will need to copy the App Registration's App (client) ID and set it in the Azure Web App appSetting called `Authentication:Microsoft:ClientId`
   4. Should you wish to enable [Facebook authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/facebook-logins?view=aspnetcore-3.1) or [Google authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-3.1), the web application is already setup and supports these. Follow the guides linked and the add the following:
      1. For Facebook authentication
         1. Set the `Authentication:Facebook:Enabled` appSetting in the Azure Web App to `true`.
         2. The redirect uri will be `https://\<webappname\>.azurewebsites.net/signin-facebook`.
         3. You will need to set the Azure Web App appSettings called `Authentication:Facebook:AppId` and `Authentication:Facebook:AppSecret` with the respective values obtain from adding a Facebook application.
      2. For Google authentication
         1. Set the `Authentication:Google:Enabled` appSetting in the Azure Web App to `true`. 
         2. The redirect uri will be `https://\<webappname\>.azurewebsites.net/signin-google`.
         3. You will need to set the Azure Web App appSettings called `Authentication:Google:ClientId` and `Authentication:Google:ClientSecret` with the respective values obtain from adding Google API.