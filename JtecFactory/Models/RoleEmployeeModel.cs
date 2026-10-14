using System;

namespace JtecFactory.Models
{
    public class RoleEmployeeModel
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
