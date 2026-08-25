using AutoMapper;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.Application.Mappings;

/// <summary>
/// AutoMapper profile defining entity-to-DTO and DTO-to-entity mappings.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Category Mappings
        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.ArticlesCount, opt => opt.MapFrom(s => s.Articles.Count))
            .ForMember(d => d.BooksCount, opt => opt.MapFrom(s => s.Books.Count))
            .ForMember(d => d.CoursesCount, opt => opt.MapFrom(s => s.Courses.Count));
        CreateMap<CreateCategoryDto, Category>();

        // Article Mappings
        CreateMap<Article, ArticleDto>()
            .ForMember(d => d.CategoryTitle, opt => opt.MapFrom(s => s.Category != null ? s.Category.Title : string.Empty))
            .ForMember(d => d.CategorySlug, opt => opt.MapFrom(s => s.Category != null ? s.Category.Slug : string.Empty));
        CreateMap<CreateArticleDto, Article>();
        CreateMap<UpdateArticleDto, Article>();

        // Book Mappings
        CreateMap<Book, BookDto>()
            .ForMember(d => d.CategoryTitle, opt => opt.MapFrom(s => s.Category != null ? s.Category.Title : string.Empty))
            .ForMember(d => d.CategorySlug, opt => opt.MapFrom(s => s.Category != null ? s.Category.Slug : string.Empty));
        CreateMap<CreateBookDto, Book>();
        CreateMap<UpdateBookDto, Book>();

        // Course Mappings
        CreateMap<Course, CourseDto>()
            .ForMember(d => d.CategoryTitle, opt => opt.MapFrom(s => s.Category != null ? s.Category.Title : string.Empty))
            .ForMember(d => d.CategorySlug, opt => opt.MapFrom(s => s.Category != null ? s.Category.Slug : string.Empty));
        CreateMap<CreateCourseDto, Course>();
        CreateMap<UpdateCourseDto, Course>();

        // Comment Mappings
        CreateMap<Comment, CommentDto>()
            .ForMember(d => d.Replies, opt => opt.MapFrom(s => s.Replies));
        CreateMap<CreateCommentDto, Comment>();

        // Admin User Mappings
        CreateMap<AdminUser, AdminUserDto>();
    }
}
