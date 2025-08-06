using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Transactions.ErrorMessages;

public class TransactionErrors
{
    public const string TransactionNotFound = "Transaction not found";
    public const string TransactionAlreadyExists = "Transaction already exists";
    public const string TransactionNotValid = "Transaction is not valid";
}

public class PaymentGatewayErrors
{
    public const string PaymentGatewayNotFound = "Payment gateway not found";
    public const string PaymentGatewayAlreadyExists = "Payment gateway already exists";
    public const string PaymentGatewayNotValid = "Payment gateway is not valid";
}