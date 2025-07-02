using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JtechnApi.Employees.Models;

namespace JtechnApi.Exams.Models
{
    [Table("exams")] // map đến bảng tên cụ thể trong MySQL
    public class Exam
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("sub_dept")]
        public int Subdept { get; set; }
        
        [Column("cycle_name")]
        public int CycleName { get; set; }
        
        [Column("create_date")]
        public DateTime CreateDate { get; set; }
        
        [Column("results")]
        public int Results { get; set; }
        
        [Column("total_questions")]
        public int TotalQuestions { get; set; }
        
        [Column("status")]
        public int Status { get; set; }
        
        [Column("confirm")]
        public int Confirm { get; set; }
        [Column("counting_time")]
        public string Counting_time { get; set; }
        
        [Column("limit_time")]
        public string Limit_time { get; set; }
        
        [Column("data")]
        public string Data { get; set; }
        
        [Column("created_at")]
        public DateTime Created_at { get; set; }
        
        [Column("updated_at")]
        public DateTime Updated_at { get; set; }
        
        [Column("deleted_at")]
        public DateTime? Deleted_at { get; set; }

        [Column("updated_by")]
        public int? Updated_by { get; set; } = 0;
        
        [Column("mission")]
        public int Mission { get; set; }
        
        [Column("scores")]
        public int Scores { get; set; }
        
        [Column("examinations")]
        public int Examinations { get; set; }
        
        [Column("date_examinations")]
        public string Date_examinations { get; set; }
        
        [Column("type")]
        public int Type { get; set; }
        
        [Column("fail_aws")]
        public string Fail_aws { get; set; }
        
        [Column("newbie")]
        public int? Newbie { get; set; } = 0;
        public Employee Employee { get; set; }
    }

}
