namespace CusomMapOSM_Application.Models.Templates.Email;

public static class EmailTemplates
{
    public static class Organization
    {
        public static string GetInvitationTemplate(string inviterName, string orgName, string memberType)
        {
            return $@"
                <h2>You've been invited to join an organization!</h2>
                <p>Hello,</p>
                <p>You have been invited by <strong>{inviterName}</strong> to join <strong>{orgName}</strong> as a <strong>{memberType}</strong>.</p>
                <p>Please log in to your account to accept or decline this invitation.</p>
                <p>If you have any questions, please contact the organization administrator.</p>
                <p>Best regards,<br>Custom Map OSM Team</p>";
        }

        public static string GetMemberRemovedTemplate(string memberName, string orgName)
        {
            return $@"
                <h2>You have been removed from an organization</h2>
                <p>Hello {memberName},</p>
                <p>You have been removed from the organization <strong>{orgName}</strong>.</p>
                <p>If you have any questions, please contact the organization administrator.</p>
                <p>Best regards,<br>Custom Map OSM Team</p>";
        }

        public static string GetOwnershipTransferTemplate(string newOwnerName, string orgName)
        {
            return $@"
                <h2>Organization ownership transferred</h2>
                <p>Hello {newOwnerName},</p>
                <p>You are now the owner of <strong>{orgName}</strong>.</p>
                <p>You have full administrative rights to manage this organization.</p>
                <p>Best regards,<br>Custom Map OSM Team</p>";
        }
    }

    public static class Authentication
    {
        public static string GetWelcomeTemplate(string userName)
        {
            return $@"
                <h2>Welcome to Custom Map OSM!</h2>
                <p>Hello {userName},</p>
                <p>Welcome to our platform! Your account has been successfully created.</p>
                <p>You can now start exploring our mapping features.</p>
                <p>Best regards,<br>Custom Map OSM Team</p>";
        }

        public static string GetPasswordResetTemplate(string resetLink, string userName)
        {
            return $@"
                <h2>Password Reset Request</h2>
                <p>Hello {userName},</p>
                <p>We received a request to reset your password.</p>
                <p>Click the link below to reset your password:</p>
                <p><a href=""{resetLink}"">Reset Password</a></p>
                <p>If you didn't request this, please ignore this email.</p>
                <p>Best regards,<br>Custom Map OSM Team</p>";
        }

        public static string GetEmailVerificationOtpTemplate(string otp)
        {
            return $@"
                <h2>Verify Your Email Address</h2>
                <p>Hello,</p>
                <p>Thank you for registering with Custom Map OSM!</p>
                <p>To complete your registration, please verify your email address using the OTP below:</p>
                <div style=""text-align: center; margin: 20px 0;"">
                    <span style=""background-color: #f0f0f0; padding: 10px 20px; font-size: 24px; font-weight: bold; letter-spacing: 2px; border-radius: 4px;"">{otp}</span>
                </div>
                <p>This OTP will expire in 10 minutes.</p>
                <p>If you didn't request this verification, please ignore this email.</p>
                <p>Best regards,<br>Custom Map OSM Team</p>";
        }

        public static string GetPasswordResetOtpTemplate(string otp)
        {
            return $@"
                <h2>Password Reset Request</h2>
                <p>Hello,</p>
                <p>We received a request to reset your password for your Custom Map OSM account.</p>
                <p>Please use the following OTP to reset your password:</p>
                <div style=""text-align: center; margin: 20px 0;"">
                    <span style=""background-color: #f0f0f0; padding: 10px 20px; font-size: 24px; font-weight: bold; letter-spacing: 2px; border-radius: 4px;"">{otp}</span>
                </div>
                <p>This OTP will expire in 10 minutes.</p>
                <p>If you didn't request this password reset, please ignore this email or contact support if you have concerns.</p>
                <p>Best regards,<br>Custom Map OSM Team</p>";
        }
    }

    public static class Membership
    {
        public static string GetSubscriptionExpiredTemplate(string userName, string planName)
        {
            return $@"
                <h2>Subscription Expired</h2>
                <p>Hello {userName},</p>
                <p>Your <strong>{planName}</strong> subscription has expired.</p>
                <p>Please renew your subscription to continue using our premium features.</p>
                <p>Best regards,<br>Custom Map OSM Team</p>";
        }

        public static string GetSubscriptionRenewedTemplate(string userName, string planName, DateTime expiryDate)
        {
            return $@"
                <h2>Subscription Renewed</h2>
                <p>Hello {userName},</p>
                <p>Your <strong>{planName}</strong> subscription has been renewed successfully.</p>
                <p>Your new expiry date is: <strong>{expiryDate:yyyy-MM-dd}</strong></p>
                <p>Best regards,<br>Custom Map OSM Team</p>";
        }
    }
}