using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Image_Sorter_DotNet.Models
{
    public class Images
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string FileName { get; set; }

        public required string FilePathName { get; set; }

        public required DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [AllowNull]
        public virtual ICollection<CollectionConnections> CollectionConnection { get; set; }
    }

    public class Collections
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string CollectionName { get; set; }

        public required DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [AllowNull]
        public virtual ICollection<CollectionConnections> CollectionConnection { get; set; }
    }

    public class CollectionConnections
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required int ImageId { get; set; }

        public required int CollectionId { get; set; }

        public required string Index { get; set; }

        public required string AltIndex { get; set; }

        public required DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public required virtual Images Image { get; set; }
        public required virtual Collections Collection { get; set; }
    }
}
