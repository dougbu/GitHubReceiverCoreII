﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace GitHubReceiverCore
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var builder = services.AddMvcCore();
            builder
                .AddAzureAlertWebHooks()
                .AddBitbucketWebHooks()
                .AddDropboxWebHooks()
                .AddDynamicsCRMWebHooks()
                .AddGitHubWebHooks()
                .AddKuduWebHooks()
                .AddMailChimpWebHooks()
                .AddPusherWebHooks()
                .AddSalesforceWebHooks()
                .AddSlackWebHooks()
                .AddStripeWebHooks();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
        }
    }
}
