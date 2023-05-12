﻿using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraAPI.Services.Interfaces;
using CameraCore.IRepository;
using CameraRepository.Repositories;
using CameraService.Services;
using CameraService.Services.IRepositoryServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using Microsoft.Extensions.Options;

namespace CameraAPI.Services
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddDIServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<ICameraRepository, CamerasRepository>();
            services.AddScoped<ICameraService, CameraService>();

            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICategoryService, CategoryService>();

            services.AddScoped<IOrderDetailService, OrderDetailService>();
            services.AddScoped<IOrderDetailsRepository, OrderDetailRepository>();

            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderRepository, OrderRepository>();

            services.AddScoped<IWarehouseCameraService, WarehouseCameraService>();
            services.AddScoped<IWarehouseCameraRepository, WarehouseCameraRepository>();

            services.AddScoped<IWarehouseCategoryService, WarehouseCategoryService>();
            services.AddScoped<IWarehouseCategoryRepository, WarehouseCategoryRepository>();

            services.AddSingleton<IRedisCacheService, RedisCacheService>();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "localhost:6379";
            });

            services.AddScoped<IPayPalService, PayPalService>();

            services.AddLogging();

            return services;
        }
    }
}
