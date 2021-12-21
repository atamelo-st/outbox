namespace OutboxSample.Application.Queries;

public sealed record GetAllUsersQuery
{
    public static readonly GetAllUsersQuery Instance = new GetAllUsersQuery();

    private GetAllUsersQuery()
    {
    }
}
