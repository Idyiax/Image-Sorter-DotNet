using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace Image_Sorter_DotNet.Models
{
    public class Images
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string FileName { get; set; }

        public required string FilePathName { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public virtual ICollection<CollectionConnections>? CollectionConnection { get; set; }
        public virtual ICollection<TagConnections>? TagConnections { get; set; }
    }

    public class Collections
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string CollectionName { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public virtual ICollection<CollectionConnections>? CollectionConnection { get; set; }
    }

    public class CollectionConnections
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required int ImageId { get; set; }

        public required int CollectionId { get; set; }

        public required string CollectionName { get; set; }

        public required int Index { get; set; }

        public int AltIndex { get; set; } = 0;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public virtual Images? Image { get; set; }
        public virtual Collections? Collection { get; set; }
    }

    public class Tags
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string TagName { get; set; }

        [RegularExpression(@"^#[0-9A-Fa-f]{6}$")]
        public required string ColourHex { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public virtual ICollection<TagRelations>? ParentTagRelations { get; set; }
        public virtual ICollection<TagRelations>? ChildTagRelations { get; set; }
        public virtual ICollection<TagConnections>? TagConnections { get; set; }
    }

    public class TagRelations
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required int ParentTagId { get; set; }

        public required int ChildTagId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public virtual Tags? ParentTag { get; set; }
        public virtual Tags? ChildTag { get; set; }
    }

    public class TagConnections
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required int TagId { get; set; }

        public required int ImageId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public virtual Tags? Tag { get; set; }
        public virtual Images? Image { get; set; }
    }
}
