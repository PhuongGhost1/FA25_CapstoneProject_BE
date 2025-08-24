using System;

namespace CusomMapOSM_Commons.Constant;

public static class PayPalConstant
{
    public static readonly string PAYPAL_CLIENT_ID = Environment.GetEnvironmentVariable("PAYPAL_CLIENT_ID") ??
        throw new ApplicationException("Cannot found paypal client id in environment variables");
    public static readonly string PAYPAL_SECRET = Environment.GetEnvironmentVariable("PAYPAL_SECRET") ??
        throw new ApplicationException("Cannot found paypal secret in environment variables");
}

public static class StripeConstant
{
    public static readonly string STRIPE_SECRET_KEY = Environment.GetEnvironmentVariable("STRIPE_SECRET") ??
        throw new ApplicationException("Cannot found stripe secret key in environment variables");

    public static readonly string STRIPE_PUBLISHABLE_KEY = Environment.GetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY") ??
        throw new ApplicationException("Cannot found stripe publishable key in environment variables");
}

public static class PayOsConstant
{
    public static readonly string PAYOS_CLIENT_ID = Environment.GetEnvironmentVariable("PAYOS_CLIENT_ID") ??
        throw new ApplicationException("Cannot found payos client id in environment variables");
    public static readonly string PAYOS_API_KEY = Environment.GetEnvironmentVariable("PAYOS_API_KEY") ??
        throw new ApplicationException("Cannot found payos secret in environment variables");
    public static readonly string PAYOS_CHECKSUM_KEY = Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY") ??
        throw new ApplicationException("Cannot found payos checksum key in environment variables");
}

public static class VnPayConstant
{
    public static readonly string VNPAY_TMN_CODE = Environment.GetEnvironmentVariable("VNPAY_TMN_CODE") ??
        throw new ApplicationException("Cannot found VNPay TMN code in environment variables");
    public static readonly string VNPAY_HASH_SECRET = Environment.GetEnvironmentVariable("VNPAY_HASH_SECRET") ??
        throw new ApplicationException("Cannot found VNPay hash secret in environment variables");
    public static readonly string VNPAY_URL = Environment.GetEnvironmentVariable("VNPAY_URL") ??
        "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
}