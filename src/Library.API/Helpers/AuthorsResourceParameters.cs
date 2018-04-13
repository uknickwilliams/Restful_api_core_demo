﻿using Library.API.Models;

namespace Library.API.Helpers
{
    public class AuthorsResourceParameters
    {
        private const int MaxPageSize = 20;

        private int _pageSize = MaxPageSize;
        
        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public string Genre { get; set; }

        public string SearchQuery { get; set; }

        public string OrderBy { get; set; } = nameof(AuthorDto.Name);

        public string Fields { get; set; }
    }
}