
using Resend;
using Shelfie.Models.Dto;

namespace Shelfie.Services;

public class ResendEmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _frontendUrl = "https://shelfie3d.com";
    private readonly string? _devRecipientOverride = "atomictacodev@gmail.com";
    private readonly bool _isDevelopment;

    public ResendEmailService(
        IResend resend,
        IWebHostEnvironment env)
    {
        _resend = resend;
        _isDevelopment = env.IsDevelopment();
        
        if (_isDevelopment)
        {
            _fromEmail = "onboarding@resend.dev";
            _fromName = "Shelfie (Dev)";
        }
        else
        {
            _fromEmail = "noreply@shelfie3d.com";
            _fromName = "Shelfie";
        }
    }

    public async Task<EmailResultDto> SendEmailConfirmationAsync(string toEmail, string username, string confirmationToken)
    {
        try
        {
            var confirmationUrl = $"{_frontendUrl}/confirm-email?token={confirmationToken}";
            
            var subject = "Confirm Your Email - Shelfie";
            var htmlContent = GenerateEmailConfirmationHtml(username, confirmationUrl);
            var plainTextContent = GenerateEmailConfirmationPlainText(username, confirmationUrl);

            var recipient = _isDevelopment ? _devRecipientOverride! : toEmail;

            return await SendEmailAsync(recipient, subject, htmlContent, plainTextContent);
        }
        catch (Exception ex)
        {
            return new EmailResultDto(false, ex.Message);
        }
    }

    public async Task<EmailResultDto> SendPasswordResetAsync(string toEmail, string username, string resetToken)
    {
        try
        {
            var resetUrl = $"{_frontendUrl}/reset-password?token={resetToken}";
            
            var subject = "Reset Your Password - Shelfie";
            var htmlContent = GeneratePasswordResetHtml(username, resetUrl);
            var plainTextContent = GeneratePasswordResetPlainText(username, resetUrl);

            var recipient = _isDevelopment ? _devRecipientOverride! : toEmail;

            return await SendEmailAsync(recipient, subject, htmlContent, plainTextContent);
        }
        catch (Exception ex)
        {
            return new EmailResultDto(false, ex.Message);
        }
    }

    public async Task<EmailResultDto> SendWelcomeEmailAsync(string toEmail, string username)
    {
        try
        {
            var subject = "Welcome to Shelfie!";
            var htmlContent = GenerateWelcomeEmailHtml(username);
            var plainTextContent = GenerateWelcomeEmailPlainText(username);

            var recipient = _isDevelopment ? _devRecipientOverride! : toEmail;

            return await SendEmailAsync(recipient, subject, htmlContent, plainTextContent);
        }
        catch (Exception ex)
        {
            return new EmailResultDto(false, ex.Message);
        }
    }

    private async Task<EmailResultDto> SendEmailAsync(string toEmail, string subject, string htmlContent, string plainTextContent)
    {
        try
        {
            var message = new EmailMessage();
            message.From = $"{_fromName} <{_fromEmail}>";
            message.To.Add(toEmail);
            message.Subject = subject;
            message.HtmlBody = htmlContent;
            message.TextBody = plainTextContent;

            await _resend.EmailSendAsync(message);
            
            return new EmailResultDto(true);
        }
        catch (Exception ex)
        {
            return new EmailResultDto(false, ex.Message);
        }
    }

    private string GenerateEmailConfirmationHtml(string username, string confirmationUrl)
    {
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Confirm Your Email</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .container {{
            background-color: #ffffff;
            border-radius: 8px;
            padding: 40px;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            text-align: center;
            margin-bottom: 30px;
        }}
        .logo {{
            font-size: 32px;
            font-weight: bold;
            color: #4F46E5;
        }}
        h1 {{
            color: #1F2937;
            font-size: 24px;
            margin-bottom: 20px;
        }}
        .button {{
            display: inline-block;
            background-color: #4F46E5;
            color: #ffffff !important;
            text-decoration: none;
            padding: 14px 32px;
            border-radius: 6px;
            font-weight: 600;
            margin: 20px 0;
        }}
        .button:hover {{
            background-color: #4338CA;
        }}
        .footer {{
            margin-top: 40px;
            padding-top: 20px;
            border-top: 1px solid #E5E7EB;
            text-align: center;
            color: #6B7280;
            font-size: 14px;
        }}
        .warning {{
            background-color: #FEF3C7;
            border-left: 4px solid #F59E0B;
            padding: 12px;
            margin: 20px 0;
            border-radius: 4px;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <div class=""logo"">📚 Shelfie</div>
        </div>
        
        <h1>Hi {username},</h1>
        
        <p>Welcome to Shelfie! We're excited to have you join us.</p>
        
        <p>Please confirm your email address by clicking the button below:</p>
        
        <div style=""text-align: center;"">
            <a href=""{confirmationUrl}"" class=""button"">Confirm Email Address</a>
        </div>
        
        <div class=""warning"">
            <strong>⚠️ This link will expire in 24 hours.</strong>
        </div>
        
        <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
        <p style=""word-break: break-all; color: #4F46E5;"">{confirmationUrl}</p>
        
        <p>If you didn't create an account with Shelfie, you can safely ignore this email.</p>
        
        <div class=""footer"">
            <p>This is an automated message from Shelfie. Please do not reply to this email.</p>
            <p>&copy; 2025 Shelfie. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateEmailConfirmationPlainText(string username, string confirmationUrl)
    {
        return $@"Hi {username},

Welcome to Shelfie! We're excited to have you join us.

Please confirm your email address by clicking the link below:

{confirmationUrl}

This link will expire in 24 hours.

If you didn't create an account with Shelfie, you can safely ignore this email.

---
This is an automated message from Shelfie. Please do not reply to this email.
© 2025 Shelfie. All rights reserved.";
    }

    private string GeneratePasswordResetHtml(string username, string resetUrl)
    {
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Reset Your Password</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .container {{
            background-color: #ffffff;
            border-radius: 8px;
            padding: 40px;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            text-align: center;
            margin-bottom: 30px;
        }}
        .logo {{
            font-size: 32px;
            font-weight: bold;
            color: #4F46E5;
        }}
        h1 {{
            color: #1F2937;
            font-size: 24px;
            margin-bottom: 20px;
        }}
        .button {{
            display: inline-block;
            background-color: #4F46E5;
            color: #ffffff !important;
            text-decoration: none;
            padding: 14px 32px;
            border-radius: 6px;
            font-weight: 600;
            margin: 20px 0;
        }}
        .button:hover {{
            background-color: #4338CA;
        }}
        .footer {{
            margin-top: 40px;
            padding-top: 20px;
            border-top: 1px solid #E5E7EB;
            text-align: center;
            color: #6B7280;
            font-size: 14px;
        }}
        .warning {{
            background-color: #FEF3C7;
            border-left: 4px solid #F59E0B;
            padding: 12px;
            margin: 20px 0;
            border-radius: 4px;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <div class=""logo"">📚 Shelfie</div>
        </div>
        
        <h1>Hi {username},</h1>
        
        <p>We received a request to reset your password. Click the button below to create a new password:</p>
        
        <div style=""text-align: center;"">
            <a href=""{resetUrl}"" class=""button"">Reset Password</a>
        </div>
        
        <div class=""warning"">
            <strong>⚠️ This link will expire in 1 hour.</strong>
        </div>
        
        <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
        <p style=""word-break: break-all; color: #4F46E5;"">{resetUrl}</p>
        
        <p>If you didn't request a password reset, please ignore this email or contact support if you have concerns.</p>
        
        <div class=""footer"">
            <p>This is an automated message from Shelfie. Please do not reply to this email.</p>
            <p>&copy; 2025 Shelfie. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GeneratePasswordResetPlainText(string username, string resetUrl)
    {
        return $@"Hi {username},

We received a request to reset your password. Click the link below to create a new password:

{resetUrl}

This link will expire in 1 hour.

If you didn't request a password reset, please ignore this email or contact support if you have concerns.

---
This is an automated message from Shelfie. Please do not reply to this email.
© 2025 Shelfie. All rights reserved.";
    }

    private string GenerateWelcomeEmailHtml(string username)
    {
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Welcome to Shelfie</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .container {{
            background-color: #ffffff;
            border-radius: 8px;
            padding: 40px;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            text-align: center;
            margin-bottom: 30px;
        }}
        .logo {{
            font-size: 32px;
            font-weight: bold;
            color: #4F46E5;
        }}
        h1 {{
            color: #1F2937;
            font-size: 24px;
            margin-bottom: 20px;
        }}
        .button {{
            display: inline-block;
            background-color: #4F46E5;
            color: #ffffff !important;
            text-decoration: none;
            padding: 14px 32px;
            border-radius: 6px;
            font-weight: 600;
            margin: 20px 0;
        }}
        .footer {{
            margin-top: 40px;
            padding-top: 20px;
            border-top: 1px solid #E5E7EB;
            text-align: center;
            color: #6B7280;
            font-size: 14px;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <div class=""logo"">📚 Shelfie</div>
        </div>
        
        <h1>Welcome to Shelfie, {username}! 🎉</h1>
        
        <p>Your account has been successfully created. We're thrilled to have you as part of our community!</p>
        
        <p>Get started by exploring your library and discovering new books.</p>
        
        <div style=""text-align: center;"">
            <a href=""{_frontendUrl}/library"" class=""button"">Go to My Library</a>
        </div>
        
        <p>If you have any questions or need help getting started, feel free to reach out to our support team.</p>
        
        <p>Happy reading!</p>
        
        <div class=""footer"">
            <p>This is an automated message from Shelfie. Please do not reply to this email.</p>
            <p>&copy; 2025 Shelfie. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateWelcomeEmailPlainText(string username)
    {
        return $@"Welcome to Shelfie, {username}! 🎉

Your account has been successfully created. We're thrilled to have you as part of our community!

Get started by exploring your library and discovering new books.

Visit your library: {_frontendUrl}/library

If you have any questions or need help getting started, feel free to reach out to our support team.

Happy reading!

---
This is an automated message from Shelfie. Please do not reply to this email.
© 2025 Shelfie. All rights reserved.";
    }
}
