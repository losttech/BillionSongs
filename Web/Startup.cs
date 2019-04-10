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

    using Python.Runtime;

    using tensorflow;

    public class Startup {
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory) {
            this.Configuration = configuration;
            this.LoggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; }
        public ILoggerFactory LoggerFactory { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            ILyricsGenerator lyricsGenerator =
                this.Configuration.GetValue<string>("Generator", null) == "dummy"
                ? new DummyLyrics()
                : this.CreateGradientLyrics();
            services.AddSingleton(lyricsGenerator);

            services.Configure<CookiePolicyOptions>(options => {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    this.Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
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
