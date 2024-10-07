using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NATS.Client.Core;
using Tgs.Payment.Clients;
using Tgs.Payment.NinghongPay;
using Tgs.Payment.NinghongPay.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddPaymentClient()
    .AddKeyedSingleton("Nats",
    (sp, _) => new NatsConnection(NatsOpts.Default with
    {
        Url = "nats://localhost:4222"
    }))
    .AddPhoenixToolkitsMessageQueue()
    .AddNatsMessageQueue(natsConfig => natsConfig
        .ConfigureResolveConnection(sp => sp.GetRequiredKeyedService<NatsConnection>("Nats")))
    .AddNatsGlobPatternExchange("*");

var app = builder.Build();

await app.StartAsync().ConfigureAwait(false);

var logger = app.Services.GetRequiredService<ILogger<Program>>();

var paymentClient = app.Services.GetRequiredService<IPaymentClient>();

var merchantCode = "Pay_991837";
var secretKey = "1f6786624d26a3caf570576880a13d9d";

//代收
/*
var orderNo = Guid.NewGuid();
var amount = 10000m;

var depositsettings = new NinghongPayDepositSettings(
    merchantCode,
    secretKey,
    orderNo,
    3,
    "刘洋");

var depositreturn = await paymentClient.DepositAsync(
    "tgs.payment.ninghongpay.deposit",
    amount,
    JsonSerializer.Serialize(depositsettings));
logger.LogInformation($"deposit return: {depositreturn}");*/


//代付

var orderNo = Guid.NewGuid();
var withdrawSettings = new NinghongPayWithdrawSettings(
    merchantCode,
    secretKey,
    orderNo,
    3,
    "Maybank",
    "123456789",
    "LIM LUI KIM");

var withdrawReturn = await paymentClient.WithdrawAsync(
    "tgs.payment.ninghongpay.withdraw",
    100M,
    JsonSerializer.Serialize(withdrawSettings));
logger.LogInformation($"Withdraw Return: {withdrawReturn}");

//代收訂單查詢
/*
var queryFormId = Guid.Parse("337dbbc7-d701-426e-b1d4-baf045ad7c21");

var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://app.ninghong.org")
};
var client = new NinghongPayClient(
    httpClient,
    new NinghongPayHasher(NullLogger<NinghongPayHasher>.Instance),
    NullLogger<NinghongPayClient>.Instance);
var depositQuery = await client.QueryDepositFormAsync("", queryFormId);

logger.LogInformation("{depositQuery}", depositQuery);*/

//餘額
/*
var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://app.ninghong.org")
};
var client = new NinghongPayClient(
    httpClient,
    new NinghongPayHasher(NullLogger<NinghongPayHasher>.Instance),
    
    NullLogger<NinghongPayClient>.Instance);
var balanceQuery = await client.QueryBalanceAsync("", merchantCode);

logger.LogInformation("Balance:{balanceQuery}", balanceQuery);*/