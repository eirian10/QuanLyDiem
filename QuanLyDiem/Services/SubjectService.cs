using Microsoft.EntityFrameworkCore;
using QuanLyDiem.Data;
using QuanLyDiem.Models;

namespace QuanLyDiem.Services
{
    public class SubjectService
    {
        private readonly ApplicationDbContext _context;

        public SubjectService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Subject>> GetAllAsync()
        {
            return await _context.Subjects
                .OrderBy(s => s.SubjectCode)
                .ToListAsync();
        }

        public async Task<Subject?> GetByIdAsync(int id)
        {
            return await _context.Subjects.FindAsync(id);
        }

        public async Task<(bool IsSuccess, string? ErrorMessage)> CreateAsync(Subject subject)
        {
            var validationMessage = await ValidateSubjectAsync(subject);

            if (validationMessage != null)
            {
                return (false, validationMessage);
            }

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool IsSuccess, string? ErrorMessage)> UpdateAsync(int id, Subject subject)
        {
            if (id != subject.SubjectId)
            {
                return (false, "Môn học không hợp lệ.");
            }

            var validationMessage = await ValidateSubjectAsync(subject, subject.SubjectId);

            if (validationMessage != null)
            {
                return (false, validationMessage);
            }

            _context.Subjects.Update(subject);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);

            if (subject == null)
            {
                return false;
            }

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Subjects.AnyAsync(s => s.SubjectId == id);
        }

        private async Task<string?> ValidateSubjectAsync(Subject subject, int? currentSubjectId = null)
        {
            const double epsilon = 0.0001;

            if (Math.Abs((subject.ProcessWeight + subject.FinalWeight) - 1.0) > epsilon)
            {
                return "Tổng trọng số điểm quá trình và điểm cuối kỳ phải bằng 1. Ví dụ: 0.4 + 0.6 = 1.";
            }

            var isDuplicated = await _context.Subjects.AnyAsync(s =>
                s.SubjectCode == subject.SubjectCode &&
                (!currentSubjectId.HasValue || s.SubjectId != currentSubjectId.Value)
            );

            if (isDuplicated)
            {
                return "Mã môn học đã tồn tại.";
            }

            return null;
        }
    }

}