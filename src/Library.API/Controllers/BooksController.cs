using System;
using System.Linq;
using System.Net;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly IUrlHelper _urlHelper;

        public BooksController(ILibraryRepository libraryRepository, IUrlHelper urlHelper)
        {
            _libraryRepository = libraryRepository;
            _urlHelper = urlHelper;
        }

        [HttpGet(Name = nameof(GetBooksForAuthor))]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var books = _libraryRepository.GetBooksForAuthor(authorId)
                .Select(b => new BookDto(b))
                .Select(CreateLinksForBook);

            var wrapper = new LinkedCollectionResourceWrapperDto<BookDto>(books);
            wrapper = CreateLinksForBooks(wrapper);

            return Ok(wrapper);
        }

        [HttpGet("{id}", Name = nameof(GetBookForAuthor))]
        public IActionResult GetBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var book = _libraryRepository.GetBookForAuthor(authorId, id);
            if (book == null)
                return NotFound();

            var bookDto = new BookDto(book);
            bookDto = CreateLinksForBook(bookDto);

            return Ok(bookDto);
        }

        [HttpPost(Name = nameof(CreateBookForAuthor))]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] CreateBookDto createBookDto)
        {
            if (createBookDto == null)
                return BadRequest();

            if (string.Equals(createBookDto.Title, createBookDto.Description))
                ModelState.AddModelError(nameof(CreateBookDto), "The title cannot be the same as the description");

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var book = createBookDto.ToBook(authorId);
            _libraryRepository.AddBookForAuthor(authorId, book);
            _libraryRepository.Save();

            var bookDto = new BookDto(book);
            bookDto = CreateLinksForBook(bookDto);
            return CreatedAtRoute(nameof(GetBookForAuthor), new {authorId, id = book.Id}, bookDto);
        }

        [HttpDelete("{id}", Name = nameof(DeleteBookForAuthor))]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var book = _libraryRepository.GetBookForAuthor(authorId, id);
            if (book == null)
                return NotFound();

            _libraryRepository.DeleteBook(book);
            _libraryRepository.Save();

            return NoContent();
        }

        [HttpDelete("{id}", Name = nameof(UpdateBookForAuthor))]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody]UpdateBookDto updateBookDto)
        {
            if (updateBookDto == null)
                return BadRequest();

            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            if (string.Equals(updateBookDto.Title, updateBookDto.Description))
                ModelState.AddModelError(nameof(UpdateBookDto), "The title cannot be the same as the description");

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var book = _libraryRepository.GetBookForAuthor(authorId, id);
            if (book == null)
            {
                book  = new Book()
                {
                    Id = id,
                    AuthorId = authorId,
                    Description = updateBookDto.Description,
                    Title = updateBookDto.Title
                };

                _libraryRepository.AddBookForAuthor(authorId, book);
                _libraryRepository.Save();

                return CreatedAtRoute(nameof(GetBookForAuthor), new { authorId = authorId, id = book.Id }, new BookDto(book));
            }

            book.Title = updateBookDto.Title;
            book.Description = updateBookDto.Description;

            _libraryRepository.UpdateBookForAuthor(book);
            _libraryRepository.Save();

            return NoContent();
        }

        [HttpDelete("{id}", Name = nameof(PatchBookForAuthor))]
        public IActionResult PatchBookForAuthor(Guid authorId, Guid id, [FromBody]JsonPatchDocument<UpdateBookDto> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();

            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var book = _libraryRepository.GetBookForAuthor(authorId, id);
            if (book == null)
            {
                var updateBookDto = new UpdateBookDto();
                patchDoc.ApplyTo(updateBookDto, ModelState);

                if (string.Equals(updateBookDto.Title, updateBookDto.Description))
                    ModelState.AddModelError(nameof(UpdateBookDto), "The title cannot be the same as the description");

                TryValidateModel(updateBookDto);

                if (!ModelState.IsValid)
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                book = new Book()
                {
                    Id = id,
                    AuthorId = authorId,
                    Description = updateBookDto.Description,
                    Title = updateBookDto.Title
                };

                _libraryRepository.AddBookForAuthor(authorId, book);
                _libraryRepository.Save();

                return CreatedAtRoute(nameof(GetBookForAuthor), new { authorId, id = book.Id }, new BookDto(book));
            }

            var bookToPatch = new UpdateBookDto()
            {
                Title = book.Title,
                Description = book.Description,
            };

            patchDoc.ApplyTo(bookToPatch, ModelState);

            if (string.Equals(bookToPatch.Title, bookToPatch.Description))
                ModelState.AddModelError(nameof(UpdateBookDto), "The title cannot be the same as the description");

            TryValidateModel(bookToPatch);

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            book.Description = bookToPatch.Description;
            book.Title = bookToPatch.Title;

            _libraryRepository.UpdateBookForAuthor(book);
            _libraryRepository.Save();

            return NoContent();
        }

        private BookDto CreateLinksForBook(BookDto book)
        {
            book.Links.Add(new LinkDto(_urlHelper.Link(nameof(GetBookForAuthor), new {id = book.Id}), "self", "GET"));
            book.Links.Add(new LinkDto(_urlHelper.Link(nameof(DeleteBookForAuthor), new {id = book.Id}), "delete_book", "DELETE"));
            book.Links.Add(new LinkDto(_urlHelper.Link(nameof(UpdateBookForAuthor), new {id = book.Id}), "update_book", "PUT"));
            book.Links.Add(new LinkDto(_urlHelper.Link(nameof(PatchBookForAuthor), new {id = book.Id}), "partially_update_book", "PATCH"));

            return book;
        }

        private LinkedCollectionResourceWrapperDto<BookDto> CreateLinksForBooks(LinkedCollectionResourceWrapperDto<BookDto> books)
        {
            books.Links.Add(new LinkDto(_urlHelper.Link(nameof(GetBooksForAuthor), new { }), "self", "GET"));

            return books;
        }
    }
}