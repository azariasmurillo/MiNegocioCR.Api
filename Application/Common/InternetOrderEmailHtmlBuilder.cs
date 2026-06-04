using System.Globalization;
using System.Net;
using System.Text;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;

namespace MiNegocioCR.Api.Application.Common;

/// <summary>HTML de correo al cliente (líneas USD + totales ₡, sin tipo de cambio).</summary>
public static class InternetOrderEmailHtmlBuilder
{
    private static readonly CultureInfo EsCr = CultureInfo.GetCultureInfo("es-CR");
    private static readonly CultureInfo EnUs = CultureInfo.GetCultureInfo("en-US");

    public static string Build(
        BusinessEntity business,
        InternetOrder order,
        string headline,
        string introMessage)
    {
        var safeBusiness = Enc(business.Name);
        var safeHeadline = Enc(headline);
        var safeIntro = Enc(introMessage);
        var contactName = Enc(order.Contact?.Name ?? "Cliente");
        var orderNumber = Enc(order.OrderNumber);
        var statusLabel = Enc(GetStatusLabel((InternetOrderStatus)order.Status));

        var logoBlock = BuildLogoBlock(business);
        var linesUsd = BuildLinesUsdTable(order);
        var totalsCrc = BuildTotalsCrcBlock(order);
        var advances = BuildAdvancesBlock(order);

        var sb = new StringBuilder();
        sb.Append("""
            <!DOCTYPE html>
            <html lang="es">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1" />
            </head>
            <body style="margin:0;padding:20px;background:#f1f5f9;font-family:Arial,Helvetica,sans-serif;color:#0f172a;">
            <div style="max-width:640px;margin:0 auto;background:#ffffff;border-radius:16px;border:1px solid #e2e8f0;overflow:hidden;">
            """);

        sb.Append($"""
            <div style="background:linear-gradient(135deg,#0f9d76 0%,#12b886 100%);padding:20px 24px;color:#ffffff;">
              <table width="100%" cellpadding="0" cellspacing="0" role="presentation"><tr>
                <td style="vertical-align:middle;width:80px;">{logoBlock}</td>
                <td style="vertical-align:middle;padding-left:12px;">
                  <div style="font-size:13px;opacity:0.9;">{safeBusiness}</div>
                  <div style="font-size:20px;font-weight:700;margin-top:4px;">{safeHeadline}</div>
                  <div style="font-size:13px;margin-top:6px;">Pedido #{orderNumber} · {statusLabel}</div>
                </td>
              </tr></table>
            </div>
            """);

        sb.Append($"""
            <div style="padding:24px;">
              <p style="margin:0 0 12px;font-size:15px;">Hola <strong>{contactName}</strong>,</p>
              <p style="margin:0 0 20px;font-size:14px;line-height:1.55;color:#334155;">{safeIntro}</p>
              <h3 style="margin:24px 0 10px;font-size:14px;color:#0f9d76;text-transform:uppercase;letter-spacing:0.06em;">Productos (USD)</h3>
              {linesUsd}
              <p style="margin:12px 0 4px;font-size:13px;text-align:right;"><strong>Subtotal productos (USD):</strong> {FmtUsd(order.LinesTotalUsd)}</p>
              <h3 style="margin:24px 0 10px;font-size:14px;color:#0f9d76;text-transform:uppercase;letter-spacing:0.06em;">Resumen (colones)</h3>
              {totalsCrc}
              {advances}
              <p style="margin:20px 0 0;font-size:16px;font-weight:700;text-align:right;color:#0f766e;">Saldo pendiente: {FmtCrc(order.BalanceDueCrc)}</p>
            </div>
            """);

        if (!string.IsNullOrWhiteSpace(order.CustomerNotes))
        {
            sb.Append($"""
                <div style="padding:0 24px 20px;">
                  <p style="margin:0;font-size:12px;color:#64748b;"><strong>Notas:</strong> {Enc(order.CustomerNotes)}</p>
                </div>
                """);
        }

        sb.Append($"""
            <div style="padding:16px 24px;background:#f8fafc;border-top:1px solid #e2e8f0;font-size:12px;color:#64748b;line-height:1.5;">
              <p style="margin:0;">{safeBusiness}</p>
              {BuildContactFooter(business)}
              <p style="margin:12px 0 0;">Este correo es informativo sobre tu pedido internet.</p>
            </div>
            </div>
            </body>
            </html>
            """);

        return sb.ToString();
    }

    private static string BuildLogoBlock(BusinessEntity business)
    {
        if (!string.IsNullOrWhiteSpace(business.LogoUrl))
        {
            var url = Enc(business.LogoUrl.Trim());
            return $"""<img src="{url}" alt="Logo" width="64" height="64" style="display:block;width:64px;height:64px;object-fit:contain;border-radius:12px;background:#ffffff;padding:4px;" />""";
        }

        var initials = Enc(GetInitials(business.Name));
        return $"""<div style="width:64px;height:64px;border-radius:12px;background:#ffffff;color:#0f9d76;font-size:22px;line-height:64px;text-align:center;font-weight:700;">{initials}</div>""";
    }

    private static string BuildLinesUsdTable(InternetOrder order)
    {
        var rows = order.Lines
            .OrderBy(l => l.SortOrder)
            .ThenBy(l => l.Id)
            .Select(line =>
            {
                var name = Enc(line.ProductName);
                var link = string.IsNullOrWhiteSpace(line.ProductUrl)
                    ? name
                    : $"""<a href="{Enc(line.ProductUrl.Trim())}" style="color:#0f9d76;">{name}</a>""";
                return $"""
                    <tr>
                      <td style="border:1px solid #e2e8f0;padding:8px;font-size:12px;">{link}</td>
                      <td style="border:1px solid #e2e8f0;padding:8px;font-size:12px;text-align:center;width:40px;">{line.Quantity}</td>
                      <td style="border:1px solid #e2e8f0;padding:8px;font-size:12px;text-align:right;white-space:nowrap;">{FmtUsd(line.UnitPriceUsd)}</td>
                      <td style="border:1px solid #e2e8f0;padding:8px;font-size:12px;text-align:right;white-space:nowrap;">{FmtUsd(line.LineTotalUsd)}</td>
                    </tr>
                    """;
            });

        var body = string.Concat(rows);
        if (string.IsNullOrEmpty(body))
        {
            body = """<tr><td colspan="4" style="border:1px solid #e2e8f0;padding:8px;font-size:12px;">Sin líneas registradas</td></tr>""";
        }

        return $"""
            <table width="100%" cellpadding="0" cellspacing="0" role="presentation" style="border-collapse:collapse;">
              <thead>
                <tr style="background:#f0fdf9;">
                  <th style="border:1px solid #e2e8f0;padding:8px;font-size:11px;text-align:left;">Producto</th>
                  <th style="border:1px solid #e2e8f0;padding:8px;font-size:11px;">Cant.</th>
                  <th style="border:1px solid #e2e8f0;padding:8px;font-size:11px;text-align:right;">Precio USD</th>
                  <th style="border:1px solid #e2e8f0;padding:8px;font-size:11px;text-align:right;">Total USD</th>
                </tr>
              </thead>
              <tbody>{body}</tbody>
            </table>
            """;
    }

    private static string BuildTotalsCrcBlock(InternetOrder order)
    {
        return $"""
            <table width="100%" cellpadding="0" cellspacing="0" role="presentation" style="font-size:13px;">
              {TotalRow("Total productos (₡)", order.LinesTotalCrc)}
              {TotalRow("Traída internacional (₡)", order.InternationalShippingCost)}
              {TotalRow("Flete local (₡)", order.LocalCourierCost)}
              {TotalRow("Servicio (₡)", order.ServiceFee)}
              <tr><td style="padding:8px 0;border-top:2px solid #0f9d76;font-weight:700;">Total pedido</td><td style="padding:8px 0;border-top:2px solid #0f9d76;font-weight:700;text-align:right;">{FmtCrc(order.SubtotalCrc)}</td></tr>
            </table>
            """;
    }

    private static string BuildAdvancesBlock(InternetOrder order)
    {
        var advances = order.Advances.OrderBy(a => a.PaidAt).ThenBy(a => a.Id).ToList();
        if (advances.Count == 0)
        {
            return """<p style="margin:12px 0 0;font-size:13px;color:#64748b;">Adelantos registrados: ninguno</p>""";
        }

        var rows = advances.Select(a =>
        {
            var when = a.PaidAt.ToString("dd/MM/yyyy", EsCr);
            var method = string.IsNullOrWhiteSpace(a.Method) ? "" : $" · {Enc(a.Method)}";
            return $"""<tr><td style="padding:4px 0;font-size:12px;">{when}{method}</td><td style="padding:4px 0;font-size:12px;text-align:right;">{FmtCrc(a.AmountCrc)}</td></tr>""";
        });

        return $"""
            <h4 style="margin:16px 0 8px;font-size:13px;color:#475569;">Adelantos</h4>
            <table width="100%" cellpadding="0" cellspacing="0" role="presentation">{string.Concat(rows)}</table>
            <p style="margin:8px 0 0;font-size:13px;text-align:right;"><strong>Total adelantos:</strong> {FmtCrc(order.TotalAdvancesCrc)}</p>
            """;
    }

    private static string TotalRow(string label, decimal amount) =>
        $"""<tr><td style="padding:4px 0;color:#475569;">{Enc(label)}</td><td style="padding:4px 0;text-align:right;">{FmtCrc(amount)}</td></tr>""";

    private static string BuildContactFooter(BusinessEntity business)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(business.Phone))
            parts.Add(Enc(business.Phone.Trim()));
        if (!string.IsNullOrWhiteSpace(business.PublicEmail))
            parts.Add(Enc(business.PublicEmail.Trim()));
        if (parts.Count == 0)
            return string.Empty;
        return $"""<p style="margin:4px 0 0;">{string.Join(" · ", parts)}</p>""";
    }

    private static string GetStatusLabel(InternetOrderStatus status) => status switch
    {
        InternetOrderStatus.Created => "Creada",
        InternetOrderStatus.Purchased => "Comprada",
        InternetOrderStatus.Received => "Recibida",
        InternetOrderStatus.Delivered => "Entregada",
        InternetOrderStatus.Cancelled => "Cancelada",
        _ => status.ToString()
    };

    private static string GetInitials(string name)
    {
        var parts = (name ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
            return "MN";
        if (parts.Length == 1)
            return parts[0][..Math.Min(2, parts[0].Length)].ToUpperInvariant();
        return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}";
    }

    private static string FmtCrc(decimal value) =>
        $"₡{value.ToString("N2", EsCr)}";

    private static string FmtUsd(decimal value) =>
        $"${value.ToString("N2", EnUs)}";

    private static string Enc(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
}
