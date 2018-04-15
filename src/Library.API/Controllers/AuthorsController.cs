using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly IUrlHelper _urlHelper;
        private readonly IPropertyMappingSerivce _propertyMappingSerivce;
        private readonly ITypeHelperService _typeHelperService;

        public AuthorsController(ILibraryRepository libraryRepository, IUrlHelper urlHelper, IPropertyMappingSerivce propertyMappingSerivce, ITypeHelperService typeHelperService)
        {
            _libraryRepository = libraryRepository;
            _urlHelper = urlHelper;
            _propertyMappingSerivce = propertyMappingSerivce;
            _typeHelperService = typeHelperService;
        }

        [HttpGet(Name = nameof(GetAuthors))]
        public IActionResult GetAuthors([FromQuery] AuthorsResourceParameters authorsResourceParameters)
        {
            if (!_propertyMappingSerivce.ValidMappingExistsFor<AuthorDto, Author>(authorsResourceParameters.OrderBy))
                return BadRequest();

            if (!_typeHelperService.TypeHasProperty<AuthorDto>(authorsResourceParameters.Fields))
                return BadRequest();

            var authors = _libraryRepository.GetAuthors(authorsResourceParameters);

            var paginationMetadata = new
            {
                totalCount = authors.TotalCount,
                pageSize = authors.PageSize,
                currentPage = authors.CurrentPage,
                totalPages = authors.TotalPages,
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

            var authorDtos = authors
                .Select(a => new AuthorDto(a))
                .ToList();

            var shaped = EnumerableExtensions.ShapeData(authorDtos, authorsResourceParameters.Fields)
                .Select(a =>
                {
                    var dict = (IDictionary<string, object>) a;
                    var authorLinks = CreateLinksForAuthor((Guid) dict["Id"], authorsResourceParameters.Fields);

                    dict.Add("Links", authorLinks);
                    return dict;
                });

            var links = CreateLinksForAuthors(authorsResourceParameters, authors.HasNext, authors.HasPrevious);

            return Ok(new
            {
                values = shaped,
                links = links
            });
        }

        [HttpGet("{id}", Name = nameof(GetAuthor))]
        public IActionResult GetAuthor(Guid id, [FromQuery] string fields)
        {
            if (!_typeHelperService.TypeHasProperty<AuthorDto>(fields))
                return BadRequest();

            var author = _libraryRepository.GetAuthor(id);
            if (author == null)
                return NotFound();
            
            var authorDto = new AuthorDto(author);
            var data = (IDictionary<string, object>) authorDto.ShapeData(fields);
            var links = CreateLinksForAuthor(id, fields);

            data.Add("links", links);

            return Ok(data);
        }

        [HttpPost]
        public IActionResult CreateAuthor([FromBody]CreateAuthorDto createAuthorDto)
        {
            if (createAuthorDto == null)
                return BadRequest();

            var author = createAuthorDto.ToAuthor();

            _libraryRepository.AddAuthor(author);

            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating an author failed on save.");
            }

            var authorDto = new AuthorDto(author);
            var data = (IDictionary<string, object>)authorDto.ShapeData(null);
            var links = CreateLinksForAuthor(authorDto.Id, null);

            data.Add("links", links);

            return CreatedAtRoute(nameof(GetAuthor), new {id = author.Id}, data);
        }

        [HttpPost("{id}")]
        public IActionResult BlockAuthorCreation(Guid id)
        {
            if (_libraryRepository.AuthorExists(id))
                return new StatusCodeResult(StatusCodes.Status409Conflict);

            return NotFound();
        }

        [HttpDelete("{id}", Name = nameof(DeleteAuthor))]
        public IActionResult DeleteAuthor(Guid id)
        {
            var author = _libraryRepository.GetAuthor(id);
            if (author == null)
                return NotFound();

            _libraryRepository.DeleteAuthor(author);
            _libraryRepository.Save();

            return NoContent();
        }

        private string CreateAuthorsResourceUri(AuthorsResourceParameters authorsResourceParameters, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link(nameof(GetAuthors), new
                    {
                        fields = authorsResourceParameters.Fields,
                        orderBy = authorsResourceParameters.OrderBy,
                        genre = authorsResourceParameters.Genre,
                        searchQuery = authorsResourceParameters.SearchQuery,
                        pageNumber = authorsResourceParameters.PageNumber - 1,
                        pageSize = authorsResourceParameters.PageSize
                    });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link(nameof(GetAuthors), new
                    {
                        fields = authorsResourceParameters.Fields,
                        orderBy = authorsResourceParameters.OrderBy,
                        genre = authorsResourceParameters.Genre,
                        searchQuery = authorsResourceParameters.SearchQuery,
                        pageNumber = authorsResourceParameters.PageNumber + 1,
                        pageSize = authorsResourceParameters.PageSize
                    });
                case ResourceUriType.Current:
                default:
                    return _urlHelper.Link(nameof(GetAuthors), new
                    {
                        fields = authorsResourceParameters.Fields,
                        orderBy = authorsResourceParameters.OrderBy,
                        genre = authorsResourceParameters.Genre,
                        searchQuery = authorsResourceParameters.SearchQuery,
                        pageNumber = authorsResourceParameters.PageNumber,
                        pageSize = authorsResourceParameters.PageSize
                    });
            }
        }

        private IEnumerable<LinkDto> CreateLinksForAuthor(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(new LinkDto(_urlHelper.Link(nameof(GetAuthor), new { id = id}), "self", "GET"));
            }
            else
            {
                links.Add(new LinkDto(_urlHelper.Link(nameof(GetAuthor), new { id = id, fields = fields }), "self", "GET"));
            }


            links.Add(new LinkDto(_urlHelper.Link(nameof(DeleteAuthor), new { id = id}), "delete_author", "DELETE"));
            links.Add(new LinkDto(_urlHelper.Link(nameof(BooksController.CreateBookForAuthor), new { authorId = id}), "create_book_for_author", "POST"));
            links.Add(new LinkDto(_urlHelper.Link(nameof(BooksController.GetBooksForAuthor), new { authorId = id}), "books", "GET"));


            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForAuthors(AuthorsResourceParameters authorsResourceParameters, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
                links.Add(new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.NextPage), "nextPage", "GET"));

            if (hasPrevious)
                links.Add(new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage), "previousPage", "GET"));

            return links;
        }
    }
}

