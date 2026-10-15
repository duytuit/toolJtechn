using System;

namespace JtecFactory.Models
{
    public interface ISoftDeletable
    {
        DateTime? deleted_at { get; set; }
    }
}
