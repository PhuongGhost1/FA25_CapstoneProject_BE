using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Tickets.ErrorMessages;

public class TicketStatusErrors
{
    public const string TicketStatusNotFound = "Ticket status not found.";
    public const string TicketStatusAlreadyExists = "Ticket status already exists.";
    public const string TicketStatusInvalid = "Ticket status is invalid.";
    public const string TicketStatusUpdateFailed = "Failed to update ticket status.";
    public const string TicketStatusDeleteFailed = "Failed to delete ticket status.";
    public const string TicketStatusCreateFailed = "Failed to create ticket status.";
}

public class SupportTicketErrors
{
    public const string TicketNotFound = "Support ticket not found.";
    public const string TicketAlreadyExists = "Support ticket already exists.";
    public const string TicketInvalid = "Support ticket is invalid.";
    public const string TicketUpdateFailed = "Failed to update support ticket.";
    public const string TicketDeleteFailed = "Failed to delete support ticket.";
    public const string TicketCreateFailed = "Failed to create support ticket.";
}
