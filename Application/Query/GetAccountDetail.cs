using Application.Storage;

namespace Application.Query;

public record GetAccountDetailQuery : IQuery
{
    public required Guid Uid { get; init; }
}

public record GetAccountDetailResponse : IQueryResponse
{
    public required Guid Uid { get; init; }
    public required string Nickname { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string BirthDate { get; init; }
}

public class GetAccountDetailHandler(
    IStorage storage
) : IQueryHandler<GetAccountDetailQuery, GetAccountDetailResponse>
{
    public async Task<GetAccountDetailResponse> HandleAsync(GetAccountDetailQuery query)
    {
        var view = await storage.GetDetailViewAsync(query.Uid);

        return new GetAccountDetailResponse
        {
            Uid = view.Uid,
            Nickname = view.Nickname,
            Email = view.Email,
            FirstName = view.FirstName,
            LastName = view.LastName,
            BirthDate = view.BirthDate
        };
    }
}
