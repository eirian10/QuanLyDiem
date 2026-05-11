using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDiem.Models.Entity
{
    public class ClassSchedule
    {
        [Key]
        public int ScheduleID { get; set; }
        public string SectionClassID { get; set; } = null!;
        public int DayOfWeek { get; set; }
        public int StartSlot { get; set; }
        public int SlotCount { get; set; }
        public string? RoomID { get; set; }

        [ForeignKey("SectionClassID")]
        public virtual SectionClass SectionClass { get; set; } = null!;
    }
}
