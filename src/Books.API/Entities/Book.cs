using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Books.API.Entities;

[Table("Books")]
public class Book
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(150)]
    public required string Title { get; set; }

    [MaxLength(2500)]
    public string? Description { get; set; }


    public Guid AuthorId { get; set; }

    public Author Author { get; set; } = null!;

    [SetsRequiredMembers]
    public Book(Guid id, Guid authorId, string title, string? description)
    {
        Id = id;
        AuthorId = authorId;
        Title = title;
        Description = description;        
    }
}
