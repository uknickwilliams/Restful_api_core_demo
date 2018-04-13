using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authorcollections")]
    public class AuthorCollectionsController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;

        public AuthorCollectionsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpPost]
        public IActionResult CreateAuthorCollection([FromBody] IEnumerable<CreateAuthorDto> authorCollection)
        {
            if (authorCollection == null)
                return BadRequest();

            var authors = authorCollection.Select(a => a.ToAuthor()).ToList();

            foreach (var author in authors)
            {
                _libraryRepository.AddAuthor(author);
            }

            _libraryRepository.Save();

            var ids = string.Join(",", authors.Select(a => a.Id.ToString()));

            var authorDtos = authors.Select(a => new AuthorDto(a));
            return CreatedAtRoute(nameof(GetAuthorCollection), new { ids = ids}, authorDtos);
        }

        [HttpGet("({ids})", Name = nameof(GetAuthorCollection))]
        public IActionResult GetAuthorCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            ids = ids?.ToList();

            if (ids == null)
                return BadRequest();

            var authors = _libraryRepository.GetAuthors(ids)
                .Select(a => new AuthorDto(a))
                .ToList();

            if (ids.Count() != authors.Count())
                return NotFound();

            return Ok(authors);
        }
    }
}
