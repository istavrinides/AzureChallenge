using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using AzureChallenge.UI.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AzureChallenge.Interfaces.Providers.Data;
using AzureChallenge.Models;
using AzureChallenge.Providers.DataProviders;
using AutoMapper;
using Microsoft.Azure.Cosmos;
using AzureChallenge.Models.Questions;
using Microsoft.Azure.Cosmos.Fluent;
using AzureChallenge.Interfaces.Providers.Questions;
using AzureChallenge.Providers;
using Microsoft.AspNetCore.Identity.UI.Services;
using AzureChallenge.UI.Services;
using AzureChallenge.UI.Areas.Identity.Data;
using AzureChallenge.Models.Tournaments;
using AzureChallenge.Models.Parameters;
using AzureChallenge.Interfaces.Providers.Tournaments;
using AzureChallenge.Interfaces.Providers.Parameters;
using System.Reflection.Metadata;
using AzureChallenge.Interfaces.Providers.REST;
using AzureChallenge.Providers.RESTProviders;

namespace AzureChallenge.UI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<AzureChallengeUIUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
            services.AddAuthorization();
            services.AddRazorPages();

            services.AddAuthentication().AddMicrosoftAccount(microsoftoptions =>
            {
                microsoftoptions.ClientId = Configuration["Authentication:Microsoft:ClientId"];
                microsoftoptions.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];
            }).AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
                facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
            }).AddGoogle(options =>
            {
                IConfigurationSection googleAuthNSection =
                    Configuration.GetSection("Authentication:Google");

                options.ClientId = googleAuthNSection["ClientId"];
                options.ClientSecret = googleAuthNSection["ClientSecret"];
            });

            services.AddTransient<IEmailSender, EmailSender>();
            services.Configure<AuthMessageSenderOptions>(Configuration);

            services.AddAutoMapper(typeof(Startup));

            var cosmosQuestionDataProvider = InitializeCosmosQuestionClientInstanceAsync(Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult();
            var cosmosTournamentDataProvider = InitializeCosmosTournamentClientInstanceAsync(Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult();
            var cosmosParameterDataProvider = InitializeCosmosParameterClientInstanceAsync(Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult();
            var cosmosAssignedQuestionDataProvider = InitializeCosmosAssignedQuestionClientInstanceAsync(Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult();

            services.AddSingleton<IDataProvider<AzureChallengeResult, Question>>(cosmosQuestionDataProvider);
            services.AddSingleton<IQuestionProvider<AzureChallengeResult, Question>, QuestionProvider>();
            services.AddSingleton<IDataProvider<AzureChallengeResult, TournamentDetails>>(cosmosTournamentDataProvider);
            services.AddSingleton<ITournamentProvider<AzureChallengeResult, TournamentDetails>, TournamentProvider>();
            services.AddSingleton<IDataProvider<AzureChallengeResult, GlobalParameters>>(cosmosParameterDataProvider);
            services.AddSingleton<IParameterProvider<AzureChallengeResult, GlobalParameters>, ParameterProvider>();
            services.AddSingleton<IDataProvider<AzureChallengeResult, AssignedQuestion>>(cosmosAssignedQuestionDataProvider);
            services.AddSingleton<IAssignedQuestionProvider<AzureChallengeResult, AssignedQuestion>, AssignedQuestionProvider>();
            services.AddSingleton<IRESTProvider, RESTProvider>();
            services.AddSingleton<IAzureAuthProvider, AzureAuthProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }

        private static async Task<CosmosDbQuestionDataProvider> InitializeCosmosQuestionClientInstanceAsync(IConfigurationSection configurationSection)
        {
            string databaseName = configurationSection.GetSection("DatabaseName").Value;
            string containerName = configurationSection.GetSection("ContainerName").Value;
            string account = configurationSection.GetSection("Account").Value;
            string key = configurationSection.GetSection("Key").Value;
            CosmosClientBuilder clientBuilder = new CosmosClientBuilder(account, key);
            CosmosClient client = clientBuilder
                                .WithConnectionModeDirect()
                                .Build();
            CosmosDbQuestionDataProvider cosmosDbService = new CosmosDbQuestionDataProvider(client, databaseName, containerName);
            DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(containerName, "/type");

            return cosmosDbService;
        }

        private static async Task<CosmosDbTournamentDataProvider> InitializeCosmosTournamentClientInstanceAsync(IConfigurationSection configurationSection)
        {
            string databaseName = configurationSection.GetSection("DatabaseName").Value;
            string containerName = configurationSection.GetSection("ContainerName").Value;
            string account = configurationSection.GetSection("Account").Value;
            string key = configurationSection.GetSection("Key").Value;
            CosmosClientBuilder clientBuilder = new CosmosClientBuilder(account, key);
            CosmosClient client = clientBuilder
                                .WithConnectionModeDirect()
                                .Build();
            CosmosDbTournamentDataProvider cosmosDbService = new CosmosDbTournamentDataProvider(client, databaseName, containerName);
            DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(containerName, "/type");

            return cosmosDbService;
        }

        private static async Task<CosmosDbParameterDataProvider> InitializeCosmosParameterClientInstanceAsync(IConfigurationSection configurationSection)
        {
            string databaseName = configurationSection.GetSection("DatabaseName").Value;
            string containerName = configurationSection.GetSection("ContainerName").Value;
            string account = configurationSection.GetSection("Account").Value;
            string key = configurationSection.GetSection("Key").Value;
            CosmosClientBuilder clientBuilder = new CosmosClientBuilder(account, key);
            CosmosClient client = clientBuilder
                                .WithConnectionModeDirect()
                                .Build();
            CosmosDbParameterDataProvider cosmosDbService = new CosmosDbParameterDataProvider(client, databaseName, containerName);
            DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(containerName, "/type");

            return cosmosDbService;
        }

        private static async Task<CosmosDbAssignedQuestionDataProvider> InitializeCosmosAssignedQuestionClientInstanceAsync(IConfigurationSection configurationSection)
        {
            string databaseName = configurationSection.GetSection("DatabaseName").Value;
            string containerName = configurationSection.GetSection("ContainerName").Value;
            string account = configurationSection.GetSection("Account").Value;
            string key = configurationSection.GetSection("Key").Value;
            CosmosClientBuilder clientBuilder = new CosmosClientBuilder(account, key);
            CosmosClient client = clientBuilder
                                .WithConnectionModeDirect()
                                .Build();
            CosmosDbAssignedQuestionDataProvider cosmosDbService = new CosmosDbAssignedQuestionDataProvider(client, databaseName, containerName);
            DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(containerName, "/type");

            return cosmosDbService;
        }
    }
}
