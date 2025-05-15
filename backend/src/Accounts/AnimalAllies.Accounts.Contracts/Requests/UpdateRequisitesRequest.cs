namespace AnimalAllies.Accounts.Contracts.Requests;

public record UpdateRequisitesRequest(IEnumerable<RequisiteRequest> Requisites);

public record RequisiteRequest(string Title, string Description);