namespace BillionSongs {
    using System;
    using System.IO;
    using System.Linq;

    using BillionSongs.Data;

    using Gradient;
    using Gradient.Samples.GPT2;

    using LostTech.WhichPython;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.UI;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

public class Startup {
    const string MsSql = "sqlsrv";
    public Startup(IConfiguration configuration, ILoggerFactory loggerFactory) {
            this.Configuration = configuration;
            this.LoggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; }
        public ILoggerFactory LoggerFactory { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddMemoryCache();
            
            ILyricsGenerator lyricsGenerator =
                this.Configuration.GetValue<string>("Generator", null) == "dummy"
                ? new DummyLyrics()
                : this.CreateGradientLyrics();
            services.AddSingleton(lyricsGenerator);

            services.Configure<IdentityOptions>(options => {
                options.Password.RequiredUniqueChars = 4;
                options.Password.RequiredLength = 5;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            });

            services.Configure<CookiePolicyOptions>(options => {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(this.ConfigureDbContext);
            services.AddDefaultIdentity<IdentityUser>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddScoped<ISongDatabase, CachedSongDatabase>();

            var serviceProvider = services.BuildServiceProvider();
            var pregenDatabase = serviceProvider.GetService<ISongDatabase>();
            var pregenDbContext = serviceProvider.GetService<ApplicationDbContext>();
            services.AddSingleton<IRandomSongProvider>(sp => new PregeneratedSongProvider(
                pregenDatabase,
                pregenDbContext.Songs,
                sp.GetService<ILogger<PregeneratedSongProvider>>(),
                sp.GetService<IApplicationLifetime>().ApplicationStopping));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }
        void ConfigureDbContext(DbContextOptionsBuilder options) {
            string connectionString = this.Configuration.GetConnectionString("DefaultConnection");
            if (this.Configuration.GetValue<string>("DB", MsSql) == MsSql)
                options.UseSqlServer(connectionString);
            else
                options.UseSqlite(connectionString);
        }

        private ILyricsGenerator CreateGradientLyrics() {
            ILyricsGenerator lyricsGenerator;
            string condaEnvName = this.Configuration.GetValue<string>("PYTHON_CONDA_ENV_NAME", null);
            if (!string.IsNullOrEmpty(condaEnvName))
                GradientSetup.UsePythonEnvironment(PythonEnvironment.EnumerateCondaEnvironments()
                    .Single(env => Path.GetFileName(env.Home) == condaEnvName));

            string checkpoint = this.Configuration.GetValue("MODEL_CHECKPOINT", "latest");
            string modelName = this.Configuration.GetValue<string>("Model:Type", null)
                ?? throw new ArgumentNullException("Model:Type");
            string runName = this.Configuration.GetValue<string>("Model:Run", null)
                ?? throw new ArgumentNullException("Model:Run");
            string gpt2Root = this.Configuration.GetValue<string>("GPT2_ROOT", null)
                ?? throw new ArgumentNullException("GPT2_ROOT");
            checkpoint = Gpt2Trainer.ProcessCheckpointConfig(gpt2Root, checkpoint, modelName: modelName, runName: runName);
            if (!File.Exists(checkpoint + ".index"))
                throw new FileNotFoundException("Can't find checkpoint " + checkpoint);
            lyricsGenerator = new GradientLyricsGenerator(
                gpt2Root: gpt2Root, modelName: modelName, checkpoint: checkpoint,
                logger: this.LoggerFactory.CreateLogger<GradientLyricsGenerator>(),
                condaEnv: condaEnvName);
            return lyricsGenerator;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            } else {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
