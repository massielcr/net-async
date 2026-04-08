using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Books.API.Entities;

[Table("Authors")]
public class Author
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(150)]
    public required string FirstName { get; set; }

    [Required]
    [MaxLength(150)]
    public required string LastName { get; set; }

    public Author() { }

    [SetsRequiredMembers]
    public Author(Guid id, string firstName, string lastName) : this()
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
    }
}
