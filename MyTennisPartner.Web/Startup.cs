using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyTennisPartner.Web.Models;
using Microsoft.AspNetCore.Identity;
using MyTennisPartner.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Logging;
using System;
using MyTennisPartner.Data.Context;
using Swashbuckle.AspNetCore.Swagger;
using MyTennisPartner.Data.Managers;
using MyTennisPartner.Web.Services;
using MyTennisPartner.Web.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using MyTennisPartner.Web.Background;
using MyTennisPartner.Web.Services.Reservations;
using MyTennisPartner.Web.Managers;
using MyTennisPartner.Models.Users;
using MyTennisPartner.Models;
using MyTennisPartner.Web;

namespace MyTennisPartner
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
        	var cs = Configuration.GetConnectionString("UserConnection");
            services.AddDbContext<UserDbContext>(options =>
                options.UseSqlServer(cs));

            // configure Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
            });

            services.AddIdentity<ApplicationUser, IdentityRole>(o =>
            {
                o.Password.RequiredLength = 6;
                o.Password.RequireUppercase = false;
                o.Password.RequireDigit = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredUniqueChars = 0;
            })
                .AddEntityFrameworkStores<UserDbContext>()
                .AddRoles<IdentityRole>()
                .AddDefaultTokenProviders();

            services.AddAuthentication().AddFacebook(facebookOptions =>
            {
                //var facebookSettings = Configuration.GetSection("FacebookSettings").Get<ExternalSecretsViewModel>();

                facebookOptions.AppId = Configuration["FacebookSettings:Id"];
                facebookOptions.AppSecret = Configuration["FacebookSettings:Secret"];
                //facebookOptions.AppId = "923078281173603";
                //facebookOptions.AppSecret = "05ef123d1e74f1483d4dd8565162cc6c";
                // this doesn't work: facebookOptions.Fields.Add("profile-pic");
            });

            services.AddAuthentication().AddGoogle(o =>
            {
                o.ClientId = Configuration["GoogleSettings:Id"];
                o.ClientSecret = Configuration["GoogleSettings:Secret"];
                o.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
                o.ClaimActions.Clear();
                o.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                o.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                o.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
                o.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
                o.ClaimActions.MapJsonKey("urn:google:profile", "link");
                o.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
            });

            services.AddAuthentication().AddTwitter(twitterOptions =>
            {
                twitterOptions.ConsumerKey = Configuration["TwitterSettings:Id"];
                twitterOptions.ConsumerSecret = Configuration["TwitterSettings:Secret"];
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(config =>
            {
                config.RequireHttpsMetadata = false;
                config.SaveToken = true;

                config.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = Configuration["jwt:issuer"],
                    ValidAudience = Configuration["jwt:issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["jwt:key"]))
                };
            });

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();
            services.Configure<EmailSenderOptions>(Configuration.GetSection("email"));
            services.Configure<JwtOptions>(Configuration.GetSection("jwt"));
            services.Configure<ScheduledJobSettings>(Configuration.GetSection("ScheduledJobSettings"));

            // Add framework services
            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);


            // database setup
            var connString = Configuration.GetConnectionString("TennisConnection");
            services
              .AddDbContext<TennisContext>(options => {
                  options.UseSqlServer(connString, o => o.EnableRetryOnFailure())
                    .EnableDetailedErrors()
                   // .EnableSensitiveDataLogging()   // todo: remove this to protect sensitive info                    
                    ;

                  // un-comment below to raise warnings to Errors when EF is evaluating locally
                  options.ConfigureWarnings(warnings =>
                      warnings.Default(WarningBehavior.Ignore)
                        .Throw(RelationalEventId.QueryClientEvaluationWarning));
              });
            services.AddScoped<TennisContext>();

            // add http clients
            services.AddSingleton<SeedHttpClient>();

            // to do: figure out best practice for RestSharp Client - singleton, or scoped?
            services.AddSingleton<CourtReservationClient>();
            services.AddSingleton<RestClientService>();

            // add managers
            services.AddScoped<LeagueManager>();
            services.AddScoped<MatchManager>();
            services.AddScoped<LineManager>();
            services.AddScoped<MemberManager>();
            services.AddScoped<ReservationManager>();
            services.AddScoped<NotificationService>();

            // add court reservation service
            services.AddSingleton<CourtReservationServiceSpareTime>();
            services.AddSingleton<CourtReservationServiceLifetime>();

            // add IUrlHelper
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(x => {
                var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
                var factory = x.GetRequiredService<IUrlHelperFactory>();
                return factory.GetUrlHelper(actionContext);
            });

            // add background service classes
            // for on-demand notifications, event driven from UI, managed with FIFO queue
            services.AddHostedService<NotificationBackgroundService>();
            services.AddSingleton<IBackgroundNotificationQueue, BackgroundNotificationQueue>();

            // add background job service
            // for recurring, regular scheduled jobs at various intervals (set by crontab in appSettings)
            services.AddHostedService<ScheduledJobsBackgroundService>();

            // admin user
            services.Configure<AdminUser>(Configuration.GetSection("AdminUser"));

            // add mvc
            services.AddMvc();

            // add http client for court reservation system
            //services.AddHttpClient<CourtReservationClient>(client =>
            //{
            //    client.BaseAddress = new Uri(ApplicationConstants.courtReservationBaseUrl);
            //    client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            //    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 agent (Windows NT 6.1)");
            //    client.DefaultRequestHeaders.Add("Host", "goldriver.tennisbookings.com");
            //    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, sdch, br");
            //    client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.8");
            //});

            if (Environment.EnvironmentName != "LocalDevelopment")
            {
                services.AddHttpsRedirection(options =>
                {
                    options.HttpsPort = 443;
                });
            }

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConfiguration(Configuration.GetSection("Logging"));
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
                loggingBuilder.AddAzureWebAppDiagnostics();
                loggingBuilder.AddEventSourceLogger();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();

            //loggerFactory.AddAzureWebAppDiagnostics(
            //    new AzureAppServicesDiagnosticsSettings
            //    {
            //        OutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss zzz} [{Level}] {RequestId}-{SourceContext}: {Message}{NewLine}{Exception}"
            //    }
            //);

            if (env is null || env.EnvironmentName == "LocalDevelopment")
            {
                Console.WriteLine("Configuring HMR...");
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                    ReactHotModuleReplacement = true,

                    // not sure if this should be here or not... did not fix HMR auto-reload issue when added
                    HotModuleReplacementEndpoint = "/dist/__webpack_hmr"
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                // Read and use headers coming from reverse proxy: X-Forwarded-For X-Forwarded-Proto
                // This is particularly important so that HttpContet.Request.Scheme will be correct behind a SSL terminating proxy
                ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto
            });

            app.UseAuthentication();

            // use our application's custom middleware for global exception handling
            app.UseMiddleware(typeof(ErrorHandlingMiddleware));

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
