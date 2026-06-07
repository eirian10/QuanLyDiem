using MailKit.Net.Smtp;
using MimeKit;
using QuanLyDiem.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    public EmailService(IConfiguration config) => _config = config;

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        // Lấy giá trị từ config, nếu null thì dùng chuỗi rỗng
        var host = _config["SmtpSettings:Host"];
        var port = _config["SmtpSettings:Port"];
        var user = _config["SmtpSettings:Username"];
        var pass = _config["SmtpSettings:Password"];

        // Kiểm tra null để tránh lỗi Runtime
        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            throw new InvalidOperationException("Cấu hình SMTP trong appsettings.json bị thiếu!");
        }

        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(user)); // Dùng user làm email gửi
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;
        email.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message };

        using var smtp = new SmtpClient();

        // Ép kiểu an toàn (Port đã được kiểm tra bằng null check phía trên)
        await smtp.ConnectAsync(host, int.Parse(port ?? "587"), MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(user, pass);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}