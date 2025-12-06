namespace CusomMapOSM_Application.Models.Templates.Email;

public static class EmailTemplates
{
    public static string GetEmailWrapper(string content, string title = "IMOS")
    {
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{title}</title>
</head>
<body style=""margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f5f5f5;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse; background-color: #f5f5f5;"">
        <tr>
            <td align=""center"" style=""padding: 40px 20px;"">
                <table role=""presentation"" style=""max-width: 600px; width: 100%; border-collapse: collapse; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                    <tr>
                        <td style=""padding: 40px 40px 30px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); border-radius: 8px 8px 0 0;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 28px; font-weight: 600; text-align: center;"">IMOS</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 40px 40px 30px;"">
                            {content}
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 30px 40px; background-color: #f9f9f9; border-radius: 0 0 8px 8px; border-top: 1px solid #e0e0e0;"">
                            <p style=""margin: 0; color: #666666; font-size: 14px; text-align: center; line-height: 1.6;"">
                                Best regards,<br>
                                <strong style=""color: #333333;"">IMOS Team</strong>
                            </p>
                            <p style=""margin: 15px 0 0; color: #999999; font-size: 12px; text-align: center;"">
                                © {DateTime.UtcNow.Year} IMOS. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    public static class Organization
    {
        public static string GetInvitationTemplate(string inviterName, string orgName, string memberType)
        {
            var content = $@"
                            <h2 style=""margin: 0 0 20px; color: #333333; font-size: 24px; font-weight: 600;"">You've been invited to join an organization!</h2>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">Hello,</p>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                You have been invited by <strong style=""color: #667eea;"">{inviterName}</strong> to join 
                                <strong style=""color: #667eea;"">{orgName}</strong> as a <strong style=""color: #667eea;"">{memberType}</strong>.
                            </p>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Please log in to your account to accept or decline this invitation.
                            </p>
                            <p style=""margin: 0; color: #555555; font-size: 16px; line-height: 1.6;"">
                                If you have any questions, please contact the organization administrator.
                            </p>";

            return GetEmailWrapper(content, "Organization Invitation");
        }

        public static string GetMemberRemovedTemplate(string memberName, string orgName)
        {
            var content = $@"
                            <h2 style=""margin: 0 0 20px; color: #333333; font-size: 24px; font-weight: 600;"">You have been removed from an organization</h2>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">Hello {memberName},</p>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                You have been removed from the organization <strong style=""color: #667eea;"">{orgName}</strong>.
                            </p>
                            <p style=""margin: 0; color: #555555; font-size: 16px; line-height: 1.6;"">
                                If you have any questions, please contact the organization administrator.
                            </p>";

            return GetEmailWrapper(content, "Organization Update");
        }

        public static string GetOwnershipTransferTemplate(string newOwnerName, string orgName)
        {
            var content = $@"
                            <h2 style=""margin: 0 0 20px; color: #333333; font-size: 24px; font-weight: 600;"">Organization ownership transferred</h2>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">Hello {newOwnerName},</p>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                You are now the owner of <strong style=""color: #667eea;"">{orgName}</strong>.
                            </p>
                            <p style=""margin: 0; color: #555555; font-size: 16px; line-height: 1.6;"">
                                You have full administrative rights to manage this organization.
                            </p>";

            return GetEmailWrapper(content, "Ownership Transfer");
        }
    }

    public static class Authentication
    {
        public static string GetWelcomeTemplate(string userName)
        {
            var content = $@"
                            <h2 style=""margin: 0 0 20px; color: #333333; font-size: 24px; font-weight: 600;"">Welcome to IMOS!</h2>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">Hello {userName},</p>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Welcome to our platform! Your account has been successfully created.
                            </p>
                            <p style=""margin: 0; color: #555555; font-size: 16px; line-height: 1.6;"">
                                You can now start exploring our mapping features.
                            </p>";

            return GetEmailWrapper(content, "Welcome to IMOS");
        }

        public static string GetPasswordResetTemplate(string resetLink, string userName)
        {
            var content = $@"
                            <h2 style=""margin: 0 0 20px; color: #333333; font-size: 24px; font-weight: 600;"">Password Reset Request</h2>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">Hello {userName},</p>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                We received a request to reset your password.
                            </p>
                            <p style=""margin: 0 0 25px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Click the button below to reset your password:
                            </p>
                            <table role=""presentation"" style=""width: 100%; border-collapse: collapse;"">
                                <tr>
                                    <td align=""center"" style=""padding: 0;"">
                                        <a href=""{resetLink}"" style=""display: inline-block; padding: 14px 32px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px; box-shadow: 0 2px 4px rgba(102, 126, 234, 0.3);"">Reset Password</a>
                                    </td>
                                </tr>
                            </table>
                            <p style=""margin: 25px 0 0; color: #999999; font-size: 14px; line-height: 1.6;"">
                                If you didn't request this, please ignore this email.
                            </p>";

            return GetEmailWrapper(content, "Password Reset");
        }

        public static string GetEmailVerificationOtpTemplate(string otp)
        {
            var content = $@"
                            <h2 style=""margin: 0 0 20px; color: #333333; font-size: 24px; font-weight: 600;"">Verify Your Email Address</h2>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">Hello,</p>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Thank you for registering with IMOS!
                            </p>
                            <p style=""margin: 0 0 25px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                To complete your registration, please verify your email address using the OTP below:
                            </p>
                            <table role=""presentation"" style=""width: 100%; border-collapse: collapse; margin: 0 0 25px;"">
                                <tr>
                                    <td align=""center"" style=""padding: 0;"">
                                        <div style=""display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 20px 40px; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);"">
                                            <span style=""color: #ffffff; font-size: 32px; font-weight: 700; letter-spacing: 8px; font-family: 'Courier New', monospace;"">{otp}</span>
                                        </div>
                                    </td>
                                </tr>
                            </table>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                This OTP will expire in <strong style=""color: #667eea;"">10 minutes</strong>.
                            </p>
                            <p style=""margin: 0; color: #999999; font-size: 14px; line-height: 1.6;"">
                                If you didn't request this verification, please ignore this email.
                            </p>";

            return GetEmailWrapper(content, "Email Verification");
        }

        public static string GetPasswordResetOtpTemplate(string otp)
        {
            var content = $@"
                            <h2 style=""margin: 0 0 20px; color: #333333; font-size: 24px; font-weight: 600;"">Password Reset Request</h2>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">Hello,</p>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                We received a request to reset your password for your IMOS account.
                            </p>
                            <p style=""margin: 0 0 25px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Please use the following OTP to reset your password:
                            </p>
                            <table role=""presentation"" style=""width: 100%; border-collapse: collapse; margin: 0 0 25px;"">
                                <tr>
                                    <td align=""center"" style=""padding: 0;"">
                                        <div style=""display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 20px 40px; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);"">
                                            <span style=""color: #ffffff; font-size: 32px; font-weight: 700; letter-spacing: 8px; font-family: 'Courier New', monospace;"">{otp}</span>
                                        </div>
                                    </td>
                                </tr>
                            </table>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                This OTP will expire in <strong style=""color: #667eea;"">10 minutes</strong>.
                            </p>
                            <p style=""margin: 0; color: #999999; font-size: 14px; line-height: 1.6;"">
                                If you didn't request this password reset, please ignore this email or contact support if you have concerns.
                            </p>";

            return GetEmailWrapper(content, "Password Reset");
        }
    }

    public static class Membership
    {
        public static string GetSubscriptionExpiredTemplate(string userName, string planName)
        {
            var content = $@"
                            <h2 style=""margin: 0 0 20px; color: #333333; font-size: 24px; font-weight: 600;"">Subscription Expired</h2>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">Hello {userName},</p>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Your <strong style=""color: #667eea;"">{planName}</strong> subscription has expired.
                            </p>
                            <p style=""margin: 0; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Please renew your subscription to continue using our premium features.
                            </p>";

            return GetEmailWrapper(content, "Subscription Expired");
        }

        public static string GetSubscriptionRenewedTemplate(string userName, string planName, DateTime expiryDate)
        {
            var content = $@"
                            <h2 style=""margin: 0 0 20px; color: #333333; font-size: 24px; font-weight: 600;"">Subscription Renewed</h2>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">Hello {userName},</p>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Your <strong style=""color: #667eea;"">{planName}</strong> subscription has been renewed successfully.
                            </p>
                            <p style=""margin: 0; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Your new expiry date is: <strong style=""color: #667eea;"">{expiryDate:yyyy-MM-dd}</strong>
                            </p>";

            return GetEmailWrapper(content, "Subscription Renewed");
        }

        public static string GetPurchaseConfirmationTemplate(
            string userName,
            string planName,
            string organizationName,
            Guid transactionId,
            decimal amount,
            string paymentMethod,
            DateTime purchaseDate,
            string startDate,
            string endDate,
            string autoRenewal,
            string status,
            int maxMapsPerMonth,
            int exportQuota,
            int maxCustomLayers,
            int maxUsersPerOrg,
            bool prioritySupport)
        {
            var content = $@"
                            <h2 style=""margin: 0 0 20px; color: #333333; font-size: 24px; font-weight: 600;"">🎉 Welcome to IMOS!</h2>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">Dear {userName},</p>
                            
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Thank you for your purchase! Your <strong style=""color: #667eea;"">{planName}</strong> membership 
                                has been successfully activated for organization <strong style=""color: #667eea;"">{organizationName}</strong>.
                            </p>
                            
                            <div style=""background: #f7fafc; padding: 20px; border-radius: 8px; margin: 20px 0;"">
                                <h3 style=""margin: 0 0 15px; color: #333333; font-size: 18px; font-weight: 600;"">Purchase Details:</h3>
                                <ul style=""margin: 0; padding-left: 20px; color: #555555; font-size: 16px; line-height: 1.8;"">
                                    <li><strong>Transaction ID:</strong> {transactionId}</li>
                                    <li><strong>Plan:</strong> {planName}</li>
                                    <li><strong>Amount:</strong> ${amount:F2}</li>
                                    <li><strong>Payment Method:</strong> {paymentMethod}</li>
                                    <li><strong>Purchase Date:</strong> {purchaseDate:MMMM dd, yyyy 'at' h:mm tt}</li>
                                </ul>
                            </div>
                            
                            <div style=""background: #f7fafc; padding: 20px; border-radius: 8px; margin: 20px 0;"">
                                <h3 style=""margin: 0 0 15px; color: #333333; font-size: 18px; font-weight: 600;"">Membership Details:</h3>
                                <ul style=""margin: 0; padding-left: 20px; color: #555555; font-size: 16px; line-height: 1.8;"">
                                    <li><strong>Organization:</strong> {organizationName}</li>
                                    <li><strong>Start Date:</strong> {startDate}</li>
                                    <li><strong>End Date:</strong> {endDate}</li>
                                    <li><strong>Auto-renewal:</strong> {autoRenewal}</li>
                                    <li><strong>Status:</strong> {status}</li>
                                </ul>
                            </div>
                            
                            <div style=""background: #f0f9ff; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #667eea;"">
                                <h3 style=""margin: 0 0 15px; color: #333333; font-size: 18px; font-weight: 600;"">Your Plan Includes:</h3>
                                <ul style=""margin: 0; padding-left: 20px; color: #555555; font-size: 16px; line-height: 1.8;"">
                                    <li>✅ Up to {maxMapsPerMonth} maps per month</li>
                                    <li>✅ Up to {exportQuota} exports per month</li>
                                    <li>✅ Up to {maxCustomLayers} custom layers</li>
                                    <li>✅ Up to {maxUsersPerOrg} users per organization</li>
                                    <li>✅ {(prioritySupport ? "Priority" : "Standard")} support</li>
                                </ul>
                            </div>
                            
                            <table role=""presentation"" style=""width: 100%; border-collapse: collapse; margin: 25px 0;"">
                                <tr>
                                    <td align=""center"" style=""padding: 0;"">
                                        <a href=""https://yourdomain.com/dashboard"" style=""display: inline-block; padding: 14px 32px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px; box-shadow: 0 2px 4px rgba(102, 126, 234, 0.3); margin: 5px;"">Go to Dashboard</a>
                                        <a href=""https://yourdomain.com/maps/create"" style=""display: inline-block; padding: 14px 32px; background: #ffffff; color: #667eea; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px; border: 2px solid #667eea; margin: 5px;"">Create Your First Map</a>
                                    </td>
                                </tr>
                            </table>
                            
                            <div style=""background: #f7fafc; padding: 20px; border-radius: 8px; margin: 20px 0;"">
                                <h3 style=""margin: 0 0 15px; color: #333333; font-size: 18px; font-weight: 600;"">Getting Started:</h3>
                                <ol style=""margin: 0; padding-left: 20px; color: #555555; font-size: 16px; line-height: 1.8;"">
                                    <li>Log in to your dashboard</li>
                                    <li>Create your first custom map</li>
                                    <li>Invite team members to collaborate</li>
                                    <li>Upload your custom layers</li>
                                    <li>Export your maps in various formats</li>
                                </ol>
                            </div>
                            
                            <p style=""margin: 0; color: #555555; font-size: 16px; line-height: 1.6;"">
                                If you have any questions or need assistance getting started, our support team is here to help!
                            </p>
                            
                            <p style=""margin: 15px 0 0; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Welcome aboard and happy mapping!
                            </p>";

            return GetEmailWrapper(content, "Membership Purchase Confirmation");
        }

        public static string GetRenewalConfirmationTemplate(
            string userName,
            string planName,
            string organizationName,
            Guid transactionId,
            decimal amount,
            DateTime renewalDate,
            string newEndDate)
        {
            var content = $@"
                            <h2 style=""margin: 0 0 20px; color: #333333; font-size: 24px; font-weight: 600;"">✅ Membership Renewed Successfully!</h2>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">Dear {userName},</p>
                            
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Great news! Your <strong style=""color: #667eea;"">{planName}</strong> membership 
                                for organization <strong style=""color: #667eea;"">{organizationName}</strong> has been 
                                automatically renewed.
                            </p>
                            
                            <div style=""background: #f7fafc; padding: 20px; border-radius: 8px; margin: 20px 0;"">
                                <h3 style=""margin: 0 0 15px; color: #333333; font-size: 18px; font-weight: 600;"">Renewal Details:</h3>
                                <ul style=""margin: 0; padding-left: 20px; color: #555555; font-size: 16px; line-height: 1.8;"">
                                    <li><strong>Transaction ID:</strong> {transactionId}</li>
                                    <li><strong>Plan:</strong> {planName}</li>
                                    <li><strong>Amount:</strong> ${amount:F2}</li>
                                    <li><strong>Renewal Date:</strong> {renewalDate:MMMM dd, yyyy}</li>
                                    <li><strong>New End Date:</strong> {newEndDate}</li>
                                </ul>
                            </div>
                            
                            <p style=""margin: 0 0 25px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Your membership continues uninterrupted, and you can keep enjoying all the features 
                                of your current plan.
                            </p>
                            
                            <table role=""presentation"" style=""width: 100%; border-collapse: collapse; margin: 25px 0;"">
                                <tr>
                                    <td align=""center"" style=""padding: 0;"">
                                        <a href=""https://yourdomain.com/dashboard"" style=""display: inline-block; padding: 14px 32px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px; box-shadow: 0 2px 4px rgba(102, 126, 234, 0.3); margin: 5px;"">Go to Dashboard</a>
                                        <a href=""https://yourdomain.com/membership"" style=""display: inline-block; padding: 14px 32px; background: #ffffff; color: #667eea; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px; border: 2px solid #667eea; margin: 5px;"">Manage Membership</a>
                                    </td>
                                </tr>
                            </table>
                            
                            <p style=""margin: 0; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Thank you for your continued trust in IMOS!
                            </p>";

            return GetEmailWrapper(content, "Membership Renewed");
        }
    }

    public static class Notification
    {
        public static string GetTransactionCompletedTemplate(string userName, decimal amount, string planName)
        {
            var content = $@"
                            <h2 style=""margin: 0 0 20px; color: #333333; font-size: 24px; font-weight: 600;"">Payment Successful!</h2>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">Hello {userName},</p>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Great news! Your payment has been processed successfully.
                            </p>
                            
                            <div style=""background: #f7fafc; padding: 20px; border-radius: 8px; margin: 20px 0;"">
                                <h3 style=""margin: 0 0 15px; color: #333333; font-size: 18px; font-weight: 600;"">Transaction Details</h3>
                                <p style=""margin: 5px 0; color: #555555; font-size: 16px; line-height: 1.6;""><strong>Plan:</strong> {planName}</p>
                                <p style=""margin: 5px 0; color: #555555; font-size: 16px; line-height: 1.6;""><strong>Amount:</strong> ${amount:F2}</p>
                                <p style=""margin: 5px 0; color: #555555; font-size: 16px; line-height: 1.6;""><strong>Date:</strong> {DateTime.UtcNow:MMMM dd, yyyy}</p>
                            </div>
                            
                            <p style=""margin: 0 0 25px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Your membership is now active and you can access all premium features.
                            </p>
                            
                            <table role=""presentation"" style=""width: 100%; border-collapse: collapse; margin: 25px 0;"">
                                <tr>
                                    <td align=""center"" style=""padding: 0;"">
                                        <a href=""https://yourdomain.com/dashboard"" style=""display: inline-block; padding: 14px 32px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px; box-shadow: 0 2px 4px rgba(102, 126, 234, 0.3);"">Go to Dashboard</a>
                                    </td>
                                </tr>
                            </table>";

            return GetEmailWrapper(content, "Payment Successful");
        }

        public static string GetMembershipExpirationWarningTemplate(string userName, int daysRemaining, string planName)
        {
            var content = $@"
                            <h2 style=""margin: 0 0 20px; color: #333333; font-size: 24px; font-weight: 600;"">Membership Expiring Soon</h2>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">Hello {userName},</p>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Your <strong style=""color: #667eea;"">{planName}</strong> membership will expire in <strong style=""color: #f56565;"">{daysRemaining} days</strong>.
                            </p>
                            
                            <div style=""background: #fff5f5; border-left: 4px solid #f56565; padding: 20px; margin: 20px 0; border-radius: 4px;"">
                                <h3 style=""margin: 0 0 10px; color: #c53030; font-size: 18px; font-weight: 600;"">⚠️ Action Required</h3>
                                <p style=""margin: 0; color: #555555; font-size: 16px; line-height: 1.6;"">
                                    To continue enjoying premium features, please renew your membership before it expires.
                                </p>
                            </div>
                            
                            <table role=""presentation"" style=""width: 100%; border-collapse: collapse; margin: 25px 0;"">
                                <tr>
                                    <td align=""center"" style=""padding: 0;"">
                                        <a href=""https://yourdomain.com/membership/renew"" style=""display: inline-block; padding: 14px 32px; background: linear-gradient(135deg, #f56565 0%, #c53030 100%); color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px; box-shadow: 0 2px 4px rgba(245, 101, 101, 0.3);"">Renew Membership</a>
                                    </td>
                                </tr>
                            </table>";

            return GetEmailWrapper(content, "Membership Expiring Soon");
        }

        public static string GetMembershipExpiredTemplate(string userName, string planName)
        {
            var content = $@"
                            <h2 style=""margin: 0 0 20px; color: #333333; font-size: 24px; font-weight: 600;"">Membership Expired</h2>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">Hello {userName},</p>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Your <strong style=""color: #667eea;"">{planName}</strong> membership has expired. You now have limited access to features.
                            </p>
                            
                            <div style=""background: #fff5f5; border-left: 4px solid #f56565; padding: 20px; margin: 20px 0; border-radius: 4px;"">
                                <h3 style=""margin: 0 0 10px; color: #c53030; font-size: 18px; font-weight: 600;"">Limited Access</h3>
                                <p style=""margin: 0; color: #555555; font-size: 16px; line-height: 1.6;"">
                                    Some features may be restricted until you renew your membership.
                                </p>
                            </div>
                            
                            <table role=""presentation"" style=""width: 100%; border-collapse: collapse; margin: 25px 0;"">
                                <tr>
                                    <td align=""center"" style=""padding: 0;"">
                                        <a href=""https://yourdomain.com/membership/renew"" style=""display: inline-block; padding: 14px 32px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px; box-shadow: 0 2px 4px rgba(102, 126, 234, 0.3);"">Renew Membership</a>
                                    </td>
                                </tr>
                            </table>";

            return GetEmailWrapper(content, "Membership Expired");
        }

        public static string GetQuotaExceededTemplate(string userName, string quotaType, int currentUsage, int limit)
        {
            var content = $@"
                            <h2 style=""margin: 0 0 20px; color: #333333; font-size: 24px; font-weight: 600;"">Quota Exceeded</h2>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">Hello {userName},</p>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                You have exceeded your <strong style=""color: #667eea;"">{quotaType}</strong> quota for this billing period.
                            </p>
                            
                            <div style=""background: #fff5f5; border-left: 4px solid #f56565; padding: 20px; margin: 20px 0; border-radius: 4px;"">
                                <h3 style=""margin: 0 0 15px; color: #c53030; font-size: 18px; font-weight: 600;"">Usage Details</h3>
                                <p style=""margin: 5px 0; color: #555555; font-size: 16px; line-height: 1.6;""><strong>Current Usage:</strong> {currentUsage}</p>
                                <p style=""margin: 5px 0; color: #555555; font-size: 16px; line-height: 1.6;""><strong>Limit:</strong> {limit}</p>
                            </div>
                            
                            <p style=""margin: 0 0 25px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Consider upgrading your plan to increase your quota limits.
                            </p>
                            
                            <table role=""presentation"" style=""width: 100%; border-collapse: collapse; margin: 25px 0;"">
                                <tr>
                                    <td align=""center"" style=""padding: 0;"">
                                        <a href=""https://yourdomain.com/membership/upgrade"" style=""display: inline-block; padding: 14px 32px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px; box-shadow: 0 2px 4px rgba(102, 126, 234, 0.3);"">Upgrade Plan</a>
                                    </td>
                                </tr>
                            </table>";

            return GetEmailWrapper(content, "Quota Exceeded");
        }

        public static string GetQuotaWarningTemplate(string userName, string quotaType, int currentUsage, int limit,
            int percentageUsed)
        {
            var content = $@"
                            <h2 style=""margin: 0 0 20px; color: #333333; font-size: 24px; font-weight: 600;"">Quota Warning</h2>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">Hello {userName},</p>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                You've used <strong style=""color: #ed8936;"">{percentageUsed}%</strong> of your <strong style=""color: #667eea;"">{quotaType}</strong> quota for this billing period.
                            </p>
                            
                            <div style=""background: #fffaf0; border-left: 4px solid #ed8936; padding: 20px; margin: 20px 0; border-radius: 4px;"">
                                <h3 style=""margin: 0 0 15px; color: #c05621; font-size: 18px; font-weight: 600;"">Usage Progress</h3>
                                <div style=""background: #e2e8f0; border-radius: 10px; height: 24px; margin: 15px 0; overflow: hidden;"">
                                    <div style=""background: linear-gradient(135deg, #ed8936 0%, #dd6b20 100%); height: 100%; border-radius: 10px; width: {percentageUsed}%; transition: width 0.3s ease;""></div>
                                </div>
                                <p style=""margin: 10px 0 0; color: #555555; font-size: 16px; line-height: 1.6;""><strong>Used:</strong> {currentUsage} / {limit}</p>
                            </div>
                            
                            <p style=""margin: 0 0 25px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Consider monitoring your usage or upgrading your plan if you need more capacity.
                            </p>
                            
                            <table role=""presentation"" style=""width: 100%; border-collapse: collapse; margin: 25px 0;"">
                                <tr>
                                    <td align=""center"" style=""padding: 0;"">
                                        <a href=""https://yourdomain.com/dashboard/usage"" style=""display: inline-block; padding: 14px 32px; background: linear-gradient(135deg, #ed8936 0%, #dd6b20 100%); color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px; box-shadow: 0 2px 4px rgba(237, 137, 54, 0.3);"">View Usage</a>
                                    </td>
                                </tr>
                            </table>";

            return GetEmailWrapper(content, "Quota Warning");
        }

        public static string GetExportCompletedTemplate(string userName, string fileName, string fileSize,
            string downloadLink)
        {
            var content = $@"
                            <h2 style=""margin: 0 0 20px; color: #333333; font-size: 24px; font-weight: 600;"">Export Ready!</h2>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">Hello {userName},</p>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Your map export has been successfully generated and is ready for download.
                            </p>
                            
                            <div style=""background: #f7fafc; padding: 20px; border-radius: 8px; margin: 20px 0;"">
                                <h3 style=""margin: 0 0 15px; color: #333333; font-size: 18px; font-weight: 600;"">Export Details</h3>
                                <p style=""margin: 5px 0; color: #555555; font-size: 16px; line-height: 1.6;""><strong>File:</strong> {fileName}</p>
                                <p style=""margin: 5px 0; color: #555555; font-size: 16px; line-height: 1.6;""><strong>Size:</strong> {fileSize}</p>
                                <p style=""margin: 5px 0; color: #555555; font-size: 16px; line-height: 1.6;""><strong>Generated:</strong> {DateTime.UtcNow:MMMM dd, yyyy 'at' HH:mm} UTC</p>
                            </div>
                            
                            <table role=""presentation"" style=""width: 100%; border-collapse: collapse; margin: 25px 0;"">
                                <tr>
                                    <td align=""center"" style=""padding: 0;"">
                                        <a href=""{downloadLink}"" style=""display: inline-block; padding: 14px 32px; background: linear-gradient(135deg, #48bb78 0%, #38a169 100%); color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px; box-shadow: 0 2px 4px rgba(72, 187, 120, 0.3);"">Download File</a>
                                    </td>
                                </tr>
                            </table>
                            
                            <p style=""margin: 15px 0 0; color: #999999; font-size: 14px; text-align: center; line-height: 1.6;"">
                                This download link will expire in 30 days.
                            </p>";

            return GetEmailWrapper(content, "Export Completed");
        }

        public static string GetExportFailedTemplate(string userName, string fileName, string errorMessage)
        {
            var content = $@"
                            <h2 style=""margin: 0 0 20px; color: #333333; font-size: 24px; font-weight: 600;"">Export Failed</h2>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">Hello {userName},</p>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                We encountered an issue while generating your export.
                            </p>
                            
                            <div style=""background: #fff5f5; border-left: 4px solid #f56565; padding: 20px; margin: 20px 0; border-radius: 4px;"">
                                <h3 style=""margin: 0 0 15px; color: #c53030; font-size: 18px; font-weight: 600;"">Error Details</h3>
                                <p style=""margin: 5px 0; color: #555555; font-size: 16px; line-height: 1.6;""><strong>File:</strong> {fileName}</p>
                                <p style=""margin: 5px 0; color: #555555; font-size: 16px; line-height: 1.6;""><strong>Error:</strong> {errorMessage}</p>
                            </div>
                            
                            <p style=""margin: 0 0 25px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Please try again or contact support if the issue persists.
                            </p>
                            
                            <table role=""presentation"" style=""width: 100%; border-collapse: collapse; margin: 25px 0;"">
                                <tr>
                                    <td align=""center"" style=""padding: 0;"">
                                        <a href=""https://yourdomain.com/support"" style=""display: inline-block; padding: 14px 32px; background: linear-gradient(135deg, #f56565 0%, #c53030 100%); color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px; box-shadow: 0 2px 4px rgba(245, 101, 101, 0.3);"">Contact Support</a>
                                    </td>
                                </tr>
                            </table>";

            return GetEmailWrapper(content, "Export Failed");
        }

        public static string GetWelcomeTemplate(string userName)
        {
            var content = $@"
                            <h2 style=""margin: 0 0 20px; color: #333333; font-size: 24px; font-weight: 600;"">Welcome to IMOS!</h2>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">Hello {userName},</p>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                Welcome to IMOS! We're excited to have you join our community of map creators and GIS professionals.
                            </p>
                            
                            <div style=""background: #f7fafc; padding: 20px; border-radius: 8px; margin: 20px 0;"">
                                <h3 style=""margin: 0 0 15px; color: #333333; font-size: 18px; font-weight: 600;"">Getting Started</h3>
                                <ul style=""margin: 0; padding-left: 20px; color: #555555; font-size: 16px; line-height: 1.8;"">
                                    <li>Create your first custom map</li>
                                    <li>Upload your own data layers</li>
                                    <li>Collaborate with team members</li>
                                    <li>Export maps in various formats</li>
                                </ul>
                            </div>
                            
                            <table role=""presentation"" style=""width: 100%; border-collapse: collapse; margin: 25px 0;"">
                                <tr>
                                    <td align=""center"" style=""padding: 0;"">
                                        <a href=""https://yourdomain.com/dashboard"" style=""display: inline-block; padding: 14px 32px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px; box-shadow: 0 2px 4px rgba(102, 126, 234, 0.3);"">Get Started</a>
                                    </td>
                                </tr>
                            </table>";

            return GetEmailWrapper(content, "Welcome to IMOS");
        }

        public static string GetOrganizationInvitationTemplate(string userName, string organizationName,
            string inviterName)
        {
            var content = $@"
                            <h2 style=""margin: 0 0 20px; color: #333333; font-size: 24px; font-weight: 600;"">You're Invited!</h2>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">Hello {userName},</p>
                            <p style=""margin: 0 0 15px; color: #555555; font-size: 16px; line-height: 1.6;"">
                                {inviterName} has invited you to join <strong style=""color: #667eea;"">{organizationName}</strong> on IMOS.
                            </p>
                            
                            <div style=""background: #f7fafc; padding: 20px; border-radius: 8px; margin: 20px 0;"">
                                <h3 style=""margin: 0 0 15px; color: #333333; font-size: 18px; font-weight: 600;"">What you'll get:</h3>
                                <ul style=""margin: 0; padding-left: 20px; color: #555555; font-size: 16px; line-height: 1.8;"">
                                    <li>Access to shared maps and data</li>
                                    <li>Collaborative mapping tools</li>
                                    <li>Team management features</li>
                                    <li>Advanced export options</li>
                                </ul>
                            </div>
                            
                            <table role=""presentation"" style=""width: 100%; border-collapse: collapse; margin: 25px 0;"">
                                <tr>
                                    <td align=""center"" style=""padding: 0;"">
                                        <a href=""https://yourdomain.com/organization/accept"" style=""display: inline-block; padding: 14px 32px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px; box-shadow: 0 2px 4px rgba(102, 126, 234, 0.3);"">Accept Invitation</a>
                                    </td>
                                </tr>
                            </table>";

            return GetEmailWrapper(content, "Organization Invitation");
        }
    }
}