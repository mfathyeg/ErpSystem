using ErpSystem.SharedKernel.Domain;

namespace ErpSystem.Modules.Configuration.Domain.Entities;

public class CompanySettings : AuditableAggregateRoot<Guid>
{
    public string CompanyName { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Website { get; private set; } = string.Empty;
    public string TaxId { get; private set; } = string.Empty;
    public string Currency { get; private set; } = string.Empty;
    public string Timezone { get; private set; } = string.Empty;
    public string DateFormat { get; private set; } = string.Empty;
    public string? LogoUrl { get; private set; }

    private CompanySettings() { }

    public static CompanySettings Create(
        string companyName,
        string address,
        string phone,
        string email,
        string currency = "SAR",
        string timezone = "Asia/Riyadh",
        string dateFormat = "DD/MM/YYYY")
    {
        return new CompanySettings
        {
            Id = Guid.NewGuid(),
            CompanyName = companyName,
            Address = address,
            Phone = phone,
            Email = email,
            Currency = currency,
            Timezone = timezone,
            DateFormat = dateFormat
        };
    }

    public void Update(
        string companyName,
        string address,
        string phone,
        string email,
        string? website,
        string? taxId,
        string currency,
        string timezone,
        string dateFormat)
    {
        CompanyName = companyName;
        Address = address;
        Phone = phone;
        Email = email;
        Website = website ?? string.Empty;
        TaxId = taxId ?? string.Empty;
        Currency = currency;
        Timezone = timezone;
        DateFormat = dateFormat;
    }

    public void SetLogo(string logoUrl)
    {
        LogoUrl = logoUrl;
    }
}
