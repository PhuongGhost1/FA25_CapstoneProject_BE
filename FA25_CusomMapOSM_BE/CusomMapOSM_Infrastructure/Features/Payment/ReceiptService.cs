using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CusomMapOSM_Domain.Entities.Transactions;
using DomainMembership = CusomMapOSM_Domain.Entities.Memberships.Membership;
using CusomMapOSM_Application.Interfaces.Features.Payment;

namespace CusomMapOSM_Infrastructure.Features.Payment;

public class ReceiptService : IReceiptService
{
    public byte[] GenerateReceipt(Transactions transaction, DomainMembership membership)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header()
                    .Column(column =>
                    {
                        column.Item().Text("PAYMENT RECEIPT")
                            .SemiBold()
                            .FontSize(24)
                            .FontColor(Colors.Blue.Medium);

                        column.Item().PaddingTop(5).LineHorizontal(2).LineColor(Colors.Blue.Medium);
                    });

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(x =>
                    {
                        x.Spacing(20);

                        // Transaction Details Section
                        x.Item().Text("Transaction Details").SemiBold().FontSize(16).FontColor(Colors.Grey.Darken3);
                        x.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(col =>
                        {
                            col.Item().Text($"Transaction ID: {transaction.TransactionId}");
                            col.Item().Text($"Date: {transaction.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                            col.Item().Text(text =>
                            {
                                text.Span("Status: ");
                                text.Span(transaction.Status ?? "N/A")
                                    .FontColor(transaction.Status?.ToLower() == "success" ? Colors.Green.Medium : Colors.Grey.Darken1)
                                    .SemiBold();
                            });
                            col.Item().Text(text =>
                            {
                                text.Span("Amount: ");
                                text.Span($"${transaction.Amount:F2} USD").Bold().FontSize(14).FontColor(Colors.Green.Darken2);
                            });
                            if (!string.IsNullOrEmpty(transaction.TransactionReference))
                            {
                                col.Item().Text($"Reference: {transaction.TransactionReference}");
                            }
                        });

                        // Organization Details Section
                        x.Item().PaddingTop(10).Text("Organization Details").SemiBold().FontSize(16).FontColor(Colors.Grey.Darken3);
                        x.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(col =>
                        {
                            col.Item().Text($"Organization: {membership.Organization?.OrgName ?? "N/A"}");
                        });

                        // Plan Details Section
                        var plan = membership.Plan;
                        if (plan != null)
                        {
                            x.Item().PaddingTop(10).Text("Plan Details").SemiBold().FontSize(16).FontColor(Colors.Grey.Darken3);
                            x.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(col =>
                            {
                                col.Item().Text($"Plan Name: {plan.PlanName}");
                                col.Item().Text($"Monthly Price: ${plan.PriceMonthly:F2}");
                                col.Item().Text($"Duration: {plan.DurationMonths} month(s)");

                                if (!string.IsNullOrEmpty(plan.Description))
                                {
                                    col.Item().PaddingTop(5).Text($"Description: {plan.Description}");
                                }
                            });
                        }

                        // Payment Gateway Information
                        if (transaction.PaymentGateway != null)
                        {
                            x.Item().PaddingTop(10).Text("Payment Information").SemiBold().FontSize(16).FontColor(Colors.Grey.Darken3);
                            x.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(col =>
                            {
                                col.Item().Text($"Payment Gateway: {transaction.PaymentGateway.Name}");
                                if (!string.IsNullOrEmpty(transaction.PaymentGatewayOrderCode))
                                {
                                    col.Item().Text($"Order Code: {transaction.PaymentGatewayOrderCode}");
                                }
                            });
                        }

                        // Additional Notes Section (if Content exists)
                        if (!string.IsNullOrEmpty(transaction.Content))
                        {
                            x.Item().PaddingTop(10).Text("Additional Information").SemiBold().FontSize(16).FontColor(Colors.Grey.Darken3);
                            x.Item().Background(Colors.Grey.Lighten4).Padding(10).Text(transaction.Content)
                                .FontSize(9).FontColor(Colors.Grey.Darken1);
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                        col.Item().PaddingTop(10).Text(text =>
                        {
                            text.Span("Thank you for your business!").SemiBold().FontSize(12).FontColor(Colors.Blue.Medium);
                        });
                        col.Item().PaddingTop(5).Text(text =>
                        {
                            text.Span("Generated on ");
                            text.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")).FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                    });
            });
        });

        return document.GeneratePdf();
    }
}
