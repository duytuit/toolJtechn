using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Works.Models
{
    [Table("work_cronjobs")]
    public class WorkCronJob
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("model_id")]
        public int ModelId { get; set; }
        [Required]
        [Column("model")]
        public string Model { get; set; }

        [Required]
        [Column("recurrence_type")]
        public int RecurrenceType { get; set; }

        [Required]
        [Column("recurrence_interval")]
        public int RecurrenceInterval { get; set; }

        [Column("storage_id")]
        public int StorageId { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Column("next_run_at")]
        public DateTime? NextRunAt { get; set; }

        [Column("last_run_at")]
        public DateTime? LastRunAt { get; set; }

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        [Column("deleted_by")]
        public int? DeletedBy { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}