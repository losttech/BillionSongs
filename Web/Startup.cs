namespace BillionSongs {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using BillionSongs.Data;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.UI;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
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
            services.AddMemoryCache(options => {
                options.SizeLimit = this.Configuration.GetValue<long?>("MemCache:SizeLimit", null);
            });

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
            services.AddDefaultIdentity<SongsUser>()
                .AddDefaultUI()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddScoped<ISongDatabase, CachedSongDatabase>();

            services.AddSingleton(sp => {
                var scopedProvider = sp.CreateScope().ServiceProvider;
                return SongVoteCache.Load(scopedProvider.GetService<ApplicationDbContext>().Votes);
            });
            services.AddSingleton(sp => {
                var scopedProvider = sp.CreateScope().ServiceProvider;
                return new PregeneratedSongProvider(
                    scopedProvider.GetService<ISongDatabase>(),
                    scopedProvider.GetService<ApplicationDbContext>().Songs,
                    sp.GetService<ILogger<PregeneratedSongProvider>>(),
                    sp.GetService<IHostApplicationLifetime>().ApplicationStopping);
            });
            services.AddSingleton<IRandomSongProvider>(sp => new RandomSongProviderCombinator(
                new WeightedRandom<IRandomSongProvider>(
                    new Dictionary<IRandomSongProvider, int>{
                        [sp.GetRequiredService<PregeneratedSongProvider>()] = 3,
                        [new TopSongWeightedProvider(sp.GetRequiredService<SongVoteCache>())] = 1,
                    }
            ), logger: sp.GetService<ILogger<RandomSongProviderCombinator>>()));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }

        void ConfigureDbContext(DbContextOptionsBuilder options) {
            string connectionString = this.Configuration.GetConnectionString("DefaultConnection");
            if (this.Configuration.GetValue<string>("DB", MsSql) == MsSql)
                options.UseSqlServer(connectionString);
            else
                options.UseSqlite(connectionString);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
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

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapRazorPages());
        }
    }
}
