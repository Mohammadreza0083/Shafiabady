using Microsoft.Extensions.DependencyInjection;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Application.Mappings;
using ShafieiAbadi.Application.Services;

namespace ShafieiAbadi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);

        services.AddScoped<IArticleService, ArticleService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IViewTrackerService, ViewTrackerService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
