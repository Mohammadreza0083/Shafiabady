using AutoMapper;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Category
        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.ArticlesCount, opt => opt.MapFrom(s => s.Articles.Count))
            .ForMember(d => d.BooksCount, opt => opt.MapFrom(s => s.Books.Count))
            .ForMember(d => d.CoursesCount, opt => opt.MapFrom(s => s.Courses.Count));
        CreateMap<CreateCategoryDto, Category>();

        // Article
        CreateMap<Article, ArticleDto>()
            .ForMember(d => d.CategoryTitle, opt => opt.MapFrom(s => s.Category != null ? s.Category.Title : string.Empty))
            .ForMember(d => d.CommentsCount, opt => opt.MapFrom(s => s.Comments.Count));
        CreateMap<CreateArticleDto, Article>();
        CreateMap<UpdateArticleDto, Article>();

        // Book
        CreateMap<Book, BookDto>()
            .ForMember(d => d.CategoryTitle, opt => opt.MapFrom(s => s.Category != null ? s.Category.Title : string.Empty))
            .ForMember(d => d.CommentsCount, opt => opt.MapFrom(s => s.Comments.Count));
        CreateMap<CreateBookDto, Book>();
        CreateMap<UpdateBookDto, Book>();

        // Course
        CreateMap<Course, CourseDto>()
            .ForMember(d => d.CategoryTitle, opt => opt.MapFrom(s => s.Category != null ? s.Category.Title : string.Empty))
            .ForMember(d => d.CommentsCount, opt => opt.MapFrom(s => s.Comments.Count));
        CreateMap<CreateCourseDto, Course>();
        CreateMap<UpdateCourseDto, Course>();

        // Comment
        CreateMap<Comment, CommentDto>()
            .ForMember(d => d.Replies, opt => opt.MapFrom(s => s.Replies));
        CreateMap<CreateCommentDto, Comment>();

        // AdminUser
        CreateMap<AdminUser, LoginResponseDto>()
            .ForMember(d => d.Token, opt => opt.Ignore())
            .ForMember(d => d.ExpiresAt, opt => opt.Ignore());
    }
}
