using uOrgHub.Projects.Models.Enums;

namespace uOrgHub.Projects.DTOs;

public class CreateClientDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public ClientType ClientType { get; set; }
    public ClientStatus Status { get; set; } = ClientStatus.Active;
    public string? Notes { get; set; }
    /// <summary>Existing Accounts customer to bill this client as (optional).</summary>
    public Guid? CustomerId { get; set; }
}

public class UpdateClientDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public ClientType ClientType { get; set; }
    public ClientStatus Status { get; set; }
    public string? Notes { get; set; }
    /// <summary>Existing Accounts customer to bill this client as; null unlinks.</summary>
    public Guid? CustomerId { get; set; }
}

/// <summary>Create the client's Accounts customer from its own details and link it.</summary>
public class CreateCustomerFromClientDto
{
    public Guid ReceivableAccountId { get; set; }
    public int PaymentTermsDays { get; set; } = 30;
}

public class ClientResponseDto
{
    public Guid Id { get; set; }
    public string ClientCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public ClientType ClientType { get; set; }
    public ClientStatus Status { get; set; }
    public string? Notes { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public DateTime CreatedAt { get; set; }
}
