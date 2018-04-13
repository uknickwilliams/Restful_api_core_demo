using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    public class UpdateBookDto : BookForManipulationDto
    {
        [Required]
        public override string Description { get; set; }
    }
}