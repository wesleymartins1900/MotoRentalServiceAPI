using MotoRentalService.Application.Services;
using MotoRentalService.Infrastructure.Data;
using MotoRentalService.Infrastructure.Messaging;
using MotoRentalService.Infrastructure.Repositories;
using MotoRentalService.Infrastructure.Storage;
using MotoRentalService.CrossCutting.Messaging;
using MotoRentalService.CrossCutting.Storage;
using MotoRentalService.CrossCutting.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using MotoRentalService.Application.Profiles;
using MotoRentalService.Domain.Contracts.Repositories;
using MotoRentalService.Application.Services.Interfaces;
using MotoRentalService.Infrastructure.Data.Repositories;
using MotoRentalService.Domain.Calculator;
using MotoRentalService.Domain.Factories;
using FluentValidation;
using MotoRentalService.Application.Dtos;
using MotoRentalService.Application.Validators;
using MotoRentalService.Application.Validators.DeliveryPerson;
using MotoRentalService.Application.Validators.Moto;
using MotoRentalService.CrossCutting.JsonConverters;
using MotoRentalService.Application.Consumer;

namespace MotoRentalService.Api
{
    public class Startup(IConfiguration configuration)
    {
        public IConfiguration Configuration { get; } = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            // Register Services
            services.AddScoped<IMotoService, MotoService>();
            services.AddScoped<IDeliveryPersonService, DeliveryPersonService>();
            services.AddScoped<IRentalService, RentalService>();

            // Configure Validators
            services.AddTransient<IValidator<RentalMotoDto>, RentalMotoDtoValidator>();
            services.AddTransient<IValidator<RegisterDeliveryPersonDto>, RegisterDeliveryPersonDtoValidator>();
            services.AddTransient<IValidator<RegisterMotoDto>, RegisterMotoDtoValidator>();
            services.AddTransient<IValidator<UpdateMotoDto>, UpdateMotoDtoValidator>();

            // Register Repositories
            services.AddScoped<IMotoRepository, MotoRepository>();
            services.AddScoped<IDeliveryPersonRepository, DeliveryPersonRepository>();
            services.AddScoped<IRentalRepository, RentalRepository>();

            // Configure Calculators
            services.AddScoped<EarlyReturnRentalCostCalculator>();
            services.AddScoped<LateReturnRentalCostCalculator>();
            services.AddScoped<OnTimeReturnRentalCostCalculator>();

            // Configure Factory
            services.AddScoped<IRentalCostCalculatorFactory, RentalCostCalculatorFactory>();

            // Configure RabbitMQ
            services.Configure<RabbitMQConfig>(Configuration.GetSection("RabbitMQ"));
            services.AddScoped<IMessageProducer>(provider =>
            {
                var rabbitMQConfig = provider.GetRequiredService<IOptions<RabbitMQConfig>>();
                return new RabbitMqProducer(rabbitMQConfig);
            });
            services.AddScoped<MotoRegisteredConsumer>();

            // Configure DbContext
            services.AddDbContext<MotoRentalDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            // Configure MemoryCache
            services.AddMemoryCache();

            // Configure AutoMapper
            services.AddAutoMapper(typeof(MappingProfile));

            // Configure Logging
            services.AddScoped<ILoggerManager, LoggerManager>();

            // Configure Storage
            services.AddSingleton<IStorageService>(sp => new LocalStorageService(Configuration["Storage:LocalPath"]));

            // Configure Controllers
            services.AddControllers()
                    .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter()); });

            // Configure Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MotoRentalService", Version = "v1" });

                c.MapType<DateOnly>(() => new OpenApiSchema
                {
                    Type = "string",
                    Format = "date"
                });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Bearer",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                };
                c.AddSecurityDefinition("Bearer", securityScheme);

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                };
                c.AddSecurityRequirement(securityRequirement);
            });

            // Configure JWT Authentication
            var key = Encoding.ASCII.GetBytes(Configuration["Jwt:SecretKey"]);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        RequireExpirationTime = true,
                    };
                });

            // Configure Authorization Policies
            services.AddAuthorizationBuilder()
                    .AddPolicy("Admin", policy => policy.RequireRole("admin"))
                    .AddPolicy("User", policy => policy.RequireRole("user"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MotoRentalService.Api v1");
                });
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Start RabbitMQ consumer
            StartMotoRegisteredConsumer(app);
        }

        private static void StartMotoRegisteredConsumer(IApplicationBuilder app)
        {
            var serviceProvider = app.ApplicationServices;
            var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            Task.Run(async () =>
            {
                using var scope = scopeFactory.CreateScope();
                var consumer = scope.ServiceProvider.GetRequiredService<MotoRegisteredConsumer>();
                await consumer.StartConsumingAsync(); 
            });
        }
    }
}
