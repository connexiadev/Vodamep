using Bazinga.AspNetCore.Authentication.Basic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Connexia.Service.Client;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Vodamep.Api.Authentication;
using Vodamep.Api.CmdQry;
using Vodamep.Api.Engines.FileSystem;
using Vodamep.Api.Engines.SqlServer;

namespace Vodamep.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;
        private BasicAuthenticationConfiguration _authConfig;
        private readonly ILogger<Startup> _logger;

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            this._configuration = configuration;
            this._loggerFactory = loggerFactory;
            this._logger = loggerFactory.CreateLogger<Startup>();
        }

        public IConfiguration Configuration => _configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.AddRouting();

            this.ConfigureAuth(services);
            this.ConfigureEngine(services);
            this.ConfigureValidationClient(services);

            services.AddTransient<VodamepHandler>(sp => new VodamepHandler(sp.GetService<Func<IEngine>>(), !IsAuthDisabled(_authConfig), _loggerFactory.CreateLogger<VodamepHandler>(), sp.GetService<IValidationClient>()));
            services.AddSingleton<DbUpdater>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var useAuthentication = !IsAuthDisabled(_authConfig);
            if (useAuthentication)
            {
                app.UseAuthentication();
            }

            app.UseVodamep(this._logger);
        }

        private void ConfigureValidationClient(IServiceCollection services)
        {
            var validationClient = new ValidateInstitutionClient();
            validationClient.BaseUrl = _authConfig.Url;

            services.AddSingleton(typeof(IValidationClient), validationClient);
        }

        private void ConfigureEngine(IServiceCollection services)
        {
            var sqlEngineConfig = this._configuration.GetSection(nameof(SqlServerEngine)).Get<SqlServerEngineConfiguration>() ?? new SqlServerEngineConfiguration();

            if (!string.IsNullOrEmpty(sqlEngineConfig.ConnectionString))
            {
                this._logger?.LogInformation("Using SqlServerEngine");

                services.AddTransient<Func<IEngine>>(sp => () => new SqlServerEngine(sqlEngineConfig, sp.GetService<DbUpdater>(), _loggerFactory.CreateLogger<SqlServerEngine>()));
                return;
            }

            var fileEngineConfig = this._configuration.GetSection(nameof(FileEngine)).Get<FileEngineConfiguration>() ?? new FileEngineConfiguration();

            if (string.IsNullOrEmpty(fileEngineConfig?.Path))
            {
                fileEngineConfig.Path = ".";
            }

            this._logger?.LogInformation("Using FileEngine: '{path}'", fileEngineConfig.Path);
            services.AddTransient<Func<IEngine>>(sp => () => new FileEngine(fileEngineConfig, _loggerFactory.CreateLogger<FileEngine>()));
        }

        private void ConfigureAuth(IServiceCollection services)
        {
            _authConfig = _configuration.GetSection("BasicAuthentication").Get<BasicAuthenticationConfiguration>() ?? new BasicAuthenticationConfiguration();

            if (IsAuthDisabled(_authConfig))
            {
                this._logger?.LogInformation("Authentication is disabled");
                return;
            }

            if (string.Equals(_authConfig.Mode, BasicAuthenticationConfiguration.Mode_UsernameEqualsPassword, StringComparison.CurrentCultureIgnoreCase))
            {
                this._logger?.LogInformation("Using UsernameEqualsPasswordCredentialVerifier");

                services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
                    .AddBasicAuthentication(new UsernameEqualsPasswordCredentialVerifier().Verify);

                return;
            }

            if (string.Equals(_authConfig.Mode, BasicAuthenticationConfiguration.Mode_UsernamePasswordUserGroup, StringComparison.CurrentCultureIgnoreCase))
            {
                this._logger?.LogInformation("Using UsernamePasswordUserGroup");

                services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
                    .AddBasicAuthentication(new RestVerifier(_authConfig.Url).Verify);

                return;
            }

            if (!string.IsNullOrEmpty(_authConfig.Proxy))
            {
                this._logger?.LogInformation("Using ProxyAuthentication: {proxy}", _authConfig.Proxy);

                services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
                    .AddBasicAuthentication(new ProxyCredentialVerifier(new Uri(_authConfig.Proxy)).Verify);

                return;
            }

            var msg = "Authentication is not configured";

            this._logger?.LogError(msg);
            throw new Exception(msg);
        }

        private bool IsAuthDisabled(BasicAuthenticationConfiguration configuration) => string.IsNullOrEmpty(configuration?.Mode) || string.Equals(_authConfig.Mode, BasicAuthenticationConfiguration.Mode_Disabled, StringComparison.CurrentCultureIgnoreCase);
    }
}
