using System;
using System.ComponentModel.DataAnnotations;
using Library.API.Entities;

namespace Library.API.Models
{
    public abstract class BookForManipulationDto
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(500)]
        public virtual string Description { get; set; }

        public Book ToBook(Guid authorId)
        {
            return new Book()
            {
                Title = Title,
                Description = Description,
                AuthorId = authorId
            };
        }
    }
}