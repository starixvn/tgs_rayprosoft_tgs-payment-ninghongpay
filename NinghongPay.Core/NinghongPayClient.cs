using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace Tgs.Payment.NinghongPay;

public class NinghongPayClient(
    HttpClient httpClient,
    NinghongPayHasher hasher,
    ILogger<NinghongPayClient> logger)
{
    public async ValueTask<string> AuthAsync(
        string merchantCode,
        string secretKey,
        CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, object>
        {
            ["merchant_code"] = merchantCode,
            ["hash"] = hasher.ComputeHash($"{merchantCode}{secretKey}")
        };

        var response = await httpClient
            .PostAsJsonAsync("/api/v1/login", dict, cancellationToken)
            .ConfigureAwait(false);

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Auth response can't read.");

        if (response.StatusCode is HttpStatusCode.NotFound)
        {
            logger.LogError(node["error"]!.GetValue<string>());
            throw new ApiFailException(node["error"]!.GetValue<string>());
        }
        else if (response.StatusCode is HttpStatusCode.UnprocessableContent or HttpStatusCode.UnprocessableEntity)
        {
            logger.LogError(node["error"]!.GetValue<string>());
            throw new ApiFailException(node["messages"]!.AsObject().ToJsonString());
        }

        return node["token"]!.GetValue<string>();
    }

    public async ValueTask<decimal> QueryBalanceAsync(
        string token,
        string merchantCode,
        CancellationToken cancellationToken = default)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var dict = new Dictionary<string, object>
        {
            ["merchant_code"] = merchantCode,
        };

        var response = await httpClient
            .PostAsJsonAsync($"/api/v1/check-balance", dict, cancellationToken)
            .ConfigureAwait(false);

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Auth response can't read.");

        if (response.StatusCode is HttpStatusCode.NotFound)
        {
            logger.LogError(node["error"]!.GetValue<string>());
            throw new ApiFailException(node["error"]!.GetValue<string>());
        }
        else if (response.StatusCode is HttpStatusCode.UnprocessableContent or HttpStatusCode.UnprocessableEntity)
        {
            logger.LogError(node["error"]!.GetValue<string>());
            throw new ApiFailException(node["messages"]!.AsObject().ToJsonString());
        }

        if (node["success"]!.GetValue<bool>() != true)
            throw new ApiFailException();

        var data = node["balances"]!.AsArray().First()!;

        return decimal.Parse(data["amount"]!.GetValue<string>());
    }

    public async ValueTask<NinghongPayClientDepositResponse> DepositAsync(
        string token,
        DepositOrder order,
        CancellationToken cancellationToken = default)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var dict = new Dictionary<string, object>
        {
            ["method"] = order.Method,
            ["amount"] = order.Amount.ToString("#.##"),
            ["mer_tx"] = order.FormId.ToString(),
            ["currency"] = "rmb",            
        };

        if(order.RealName != null)
            dict["username"] = order.RealName;

        var response = await httpClient
            .PostAsJsonAsync("/api/v1/payin", dict, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Deposit response can't read.");

        if (node["success"]!.GetValue<bool>() != true)
            throw new ApiFailException(node["message"]!.GetValue<string>());

        var data = node["payin"];

        return new NinghongPayClientDepositResponse(
            Amount: decimal.Parse(data!["amount"]!.GetValue<string>()),
            FormId: Guid.Parse(data["mer_tx"]!.GetValue<string>()),
            ThirdPartyOrderNo: data!["txid"]!.GetValue<string>(),
            RedirectUrl: new Uri(data!["remark"]!.GetValue<string>()));
    }

    public async ValueTask<NinghongPayClientWithdrawResponse> WithdrawAsync(
        string token,
        WithdrawOrder order,
        CancellationToken cancellationToken = default)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var dict = new Dictionary<string, object>
        {
            ["method"] = order.Method,
            ["amount"] = order.Amount.ToString("#.##"),
            ["mer_tx"] = order.FormId.ToString(),
            ["bankname"] = order.BankName,
            ["accno"] = order.AccountNo,
            ["accname"] = order.AccountName,
        };

        var response = await httpClient
            .PostAsJsonAsync("/api/v1/payout", dict, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Withdraw response can't read.");

        if (node["success"]!.GetValue<bool>() != true)
            throw new ApiFailException(node["message"]!.GetValue<string>());

        var data = node["payout"]!;

        return new NinghongPayClientWithdrawResponse(
            FormId: Guid.Parse(data["mer_tx"]!.GetValue<string>()),
            ThirdPartyOrderNo: data["txid"]!.GetValue<string>(),
            Amount: decimal.Parse(data["amount"]!.GetValue<string>()));
    }

    public async ValueTask<string> QueryDepositFormAsync(
        string token,
        Guid formId,
        CancellationToken cancellationToken = default)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var dict = new Dictionary<string, object>
        {
            ["merchant_tx"] = formId,
        };

        var response = await httpClient
            .PostAsJsonAsync("/api/v1/payincheck", dict, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Query Deposit response can't read.");

        if (node["success"]!.GetValue<bool>() != true)
            throw new ApiFailException(node["message"]!.GetValue<string>());

        var data = node["payin"]!;

        return data["status"]!.GetValue<string>();
    }
}