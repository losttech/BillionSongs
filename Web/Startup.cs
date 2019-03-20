namespace BillionSongs {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.UI;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpsPolicy;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using BillionSongs.Data;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Gradient.Samples.GPT2;
    using Python.Runtime;
    using tensorflow;

    public class Startup {
        public Startup(IConfiguration configuration) {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            tf.set_random_seed(5670026);

            string checkpoint = this.Configuration.GetValue("MODEL_CHECKPOINT", "latest");
            string modelName = this.Configuration.GetValue<string>("GPT2_MODEL", null)
                ?? throw new ArgumentNullException("GPT2_MODEL");
            string runName = this.Configuration.GetValue<string>("MODEL_RUN", null)
                ?? throw new ArgumentNullException("MODEL_RUN");
            string gpt2Root = this.Configuration.GetValue<string>("GPT2_ROOT", null)
                ?? throw new ArgumentNullException("GPT2_ROOT");
            checkpoint = Gpt2Trainer.ProcessCheckpointConfig(gpt2Root, checkpoint, modelName: modelName, runName: runName);
            if (!File.Exists(checkpoint + ".index"))
                throw new FileNotFoundException("Can't find checkpoint " + checkpoint);
            services.AddSingleton<ILyricsGenerator>(new GradientLyricsGenerator(
                gpt2Root: gpt2Root, modelName: modelName, checkpoint: checkpoint));

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

            PythonEngine.BeginAllowThreads();
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
