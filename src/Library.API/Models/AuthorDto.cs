using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.Entities;
using Library.API.Helpers;

namespace Library.API.Models
{
    public class AuthorDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Genre { get; set; }

        public AuthorDto()
        {

        }

        public AuthorDto(Author author)
        {
            Id = author.Id;
            Name = string.Join(" ", author.FirstName, author.LastName);
            Age = author.DateOfBirth.GetCurrentAge();
            Genre = author.Genre;
        }
    }
}
