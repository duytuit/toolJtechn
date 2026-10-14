using System;

namespace JtecFactory.Models
{
    public interface ISoftDeletable
    {
        DateTime? DeletedAt { get; set; }
    }
}
