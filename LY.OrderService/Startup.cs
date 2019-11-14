using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace LY.OrderService
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
            services.AddControllers();
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.ApiName = Configuration["IdentityServerOptions:ApiName"];
                    options.ApiSecret = Configuration["IdentityServerOptions:ApiSecret"];
                    options.Authority = Configuration["IdentityServerOptions:Authority"];
                    options.RequireHttpsMetadata = bool.Parse(Configuration["IdentityServerOptions:RequireHttpsMetadata"]);
                });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("order", new OpenApiInfo()
                {
                    Version = "v1",
                    Title = "order api",
                    Description = "��������",
                });
                var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath + "//xmls", "LY.OrderService.xml");
                c.IncludeXmlComments(xmlPath);

                c.OperationFilter<AddResponseHeadersFilter>();
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                c.OperationFilter<SecurityRequirementsOperationFilter>();
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme()
                {
                    Description = "JWT��Ȩ(���ݽ�������ͷ�н��д���) ֱ�����¿�������Bearer {token}��ע������֮����һ���ո�\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
            });
            var csredis = new CSRedis.CSRedisClient(Configuration.GetSection("Redis:Host").Value);
            RedisHelper.Initialization(csredis);
            services.AddSingleton<IDistributedCache>(new CSRedisCache(RedisHelper.Instance));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseSwagger(c => { c.RouteTemplate = "/{documentName}/swagger.json"; });
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/order/swagger.json", "order doc"); });

            #region consul

            //����ע���Consul��ַ
            var consulClient = new ConsulClient(x => x.Address = new Uri($"http://{Configuration["Consul:IP"]}:{Configuration["Consul:Port"]}"));
            var httpCheck = new AgentServiceCheck()
            {
                DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),
                Interval = TimeSpan.FromSeconds(10),
                HTTP = $"http://{Configuration["Service:IP"]}:{Configuration["Service:Port"]}/api/health",
                Timeout = TimeSpan.FromSeconds(5)
            };
            var registration = new AgentServiceRegistration()
            {
                Checks = new[] { httpCheck },
                ID = Guid.NewGuid().ToString(),
                Name = Configuration["Service:Name"],
                Address = Configuration["Service:IP"],
                Port = int.Parse(Configuration["Service:Port"]),
                Tags = new[] { $"urlprefix-/{Configuration["Service:Name"]}" } //��� urlprefix-/servicename ��ʽ�� tag ��ǩ���Ա� Fabio ʶ��
            };
            consulClient.Agent.ServiceRegister(registration).Wait();
            lifetime.ApplicationStopping.Register(() => { consulClient.Agent.ServiceDeregister(registration.ID).Wait(); }); //����ֹͣʱȡ��ע��

            #endregion consul

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}