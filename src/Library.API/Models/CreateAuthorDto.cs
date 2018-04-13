using System;
using System.Collections.Generic;
using System.Linq;
using Library.API.Entities;

namespace Library.API.Models
{
    public class CreateAuthorDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public string Genre { get; set; }

        public ICollection<CreateBookDto> Books { get; set; } = new List<CreateBookDto>();

        public CreateAuthorDto()
        {
        }

        public CreateAuthorDto(string firstName, string lastName, DateTimeOffset dateOfBirth, string genre)
        {
            FirstName = firstName;
            LastName = lastName;
            DateOfBirth = dateOfBirth;
            Genre = genre;
        }

        public Author ToAuthor()
        {
            var authorId = Guid.NewGuid();

            return new Author()
            {
                FirstName = FirstName,
                LastName = LastName,
                DateOfBirth = DateOfBirth,
                Genre = Genre,
                Books = Books.Select(b => b.ToBook(authorId)).ToArray()
            };
        }
    }
}