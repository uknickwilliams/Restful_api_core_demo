using System;
using Library.API.Entities;

namespace Library.API.Models
{
    public class BookDto : LinkedResourceBaseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid AuthorId { get; set; }

        public BookDto()
        {

        }

        public BookDto(Book book)
        {
            Id = book.Id;
            Title = book.Title;
            Description = book.Description;
            AuthorId = book.AuthorId;
        }
    }
}