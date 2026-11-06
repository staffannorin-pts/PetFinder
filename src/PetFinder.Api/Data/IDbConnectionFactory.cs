using System.Data;

namespace PetFinder.Api.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
