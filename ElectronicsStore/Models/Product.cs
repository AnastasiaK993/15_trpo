using System;
using System.Collections.Generic;
using System.Linq;
namespace ElectronicsStore.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public double Price { get; set; }

    public int Stock { get; set; }

    public double Rating { get; set; }

    public DateOnly CreatedAt { get; set; }

    public int CategoryId { get; set; }

    public int BrandId { get; set; }
    public bool IsLowStock => Stock < 10;
    public string TagsText => Tags != null && Tags.Any()
    ? string.Join(" ", Tags.Select(t => "#" + t.Name))
    : "";

    public virtual Brand Brand { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
