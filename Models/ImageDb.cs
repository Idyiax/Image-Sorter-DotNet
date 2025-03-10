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

    public class Tags
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string TagName { get; set; }

        public required int ColourR { get; set; }
        public required int ColourG { get; set; }
        public required int ColourB { get; set; }


        public required DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [AllowNull]
        public virtual ICollection<TagRelations> ParentTagRelations { get; set; }
        [AllowNull]
        public virtual ICollection<TagRelations> ChildTagRelations { get; set; }
        [AllowNull]
        public virtual ICollection<TagConnections> TagConnections { get; set; }
    }

    public class TagRelations
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required int ParentTagId { get; set; }

        public required int ChildTagId { get; set; }

        public required DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public required Tags ParentTag { get; set; }
        public required Tags ChildTag { get; set; }
    }

    public class TagConnections
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required int TagId { get; set; }

        public required int ImageId { get; set; }

        public required DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public required Tags Tag { get; set; }
        public required Images Image { get; set; }
    }
}
