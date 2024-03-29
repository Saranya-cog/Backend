﻿using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using AzureFunctions.Extensions.Swashbuckle;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using MongoDB.Driver;
using stockmarket;
using stockmarket.Service;
using System;
using System.IO;
using System.Reflection;
using System.Security.Authentication;
using System.Threading.Tasks;


[assembly: FunctionsStartup(typeof(Startup))]
namespace stockmarket
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            builder.AddSwashBuckle(Assembly.GetExecutingAssembly());
            builder.Services.AddSingleton<IConfiguration>(config);
            var eventHubConnectionString = Environment.GetEnvironmentVariable("EventHubConnectionString");
            var eventHubFqdn = Environment.GetEnvironmentVariable("EventHubFqdn");
            var caCertFileLocation = GetCaCertFileLocation();
            var kvUri = "https://stock-market-key.vault.azure.net/";
            var cred = new ChainedTokenCredential(new ManagedIdentityCredential(), new AzureCliCredential());
            SecretClient client = new SecretClient(new Uri(kvUri), cred);
            var secret = client.GetSecretAsync("ConnectionString").Result.Value;
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(secret.Value));
           // var azureCredentialOptions = new DefaultAzureCredentialOptions();
            //var credential = new DefaultAzureCredential(azureCredentialOptions);
            //var client = new SecretClient(new Uri(kvUri), credential);
            //var secret1 = client.GetSecretAsync("ConnectionString").Result.Value;
            // MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl("mongodb://stockdb-17:GK9lufIhA2antTAlqFWmnNWx24mI8dETNuDd3BWkfo1Mv8mJ44LHnEWUn6OQedQEsgwYROurd9aL2Z5Suq3AAg==@stockdb-17.mongo.cosmos.azure.com:10255/?ssl=true&retrywrites=false&replicaSet=globaldb&maxIdleTimeMS=120000&appName=@stockdb-17@"));
            settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            builder.Services.AddSingleton((s) => new MongoClient(settings));
            builder.Services.AddSingleton<IKafkaProducer>(new KafkaProducer(eventHubFqdn, eventHubConnectionString, caCertFileLocation));
            builder.Services.AddSingleton<IKafkaConsumer>(new KafkaConsumer(eventHubFqdn, eventHubConnectionString, caCertFileLocation));
            builder.Services.AddScoped<IStockService, StockService>();
            builder.Services.AddScoped<IStockDetailService, StockDetailService>();
        }

        private string GetCaCertFileLocation()
        {
            if (Environment.GetEnvironmentVariable("IsLocal") == "1")
            {
                return "cacert.pem";
            }
            else
            {
                // When the certificate file is copied to the output directory
                // of the Azure Function, it will reside in the site/wwwroot
                // folder. 
                //string home = Environment.GetEnvironmentVariable("HOME");
                //string path = Path.Combine(home, "site", "wwwroot");
                return  "./cacert.pem";
            }
        }

    }
}

