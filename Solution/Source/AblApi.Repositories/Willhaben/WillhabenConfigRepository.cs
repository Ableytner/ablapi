using AblApi.DataAccess.Context;
using AblApi.DataAccess.Models;
using AblApi.Repositories.Interfaces.Willhaben;

namespace AblApi.Repositories.Willhaben;

public class WillhabenConfigRepository(AblContext context) : GenericRepository<WillhabenConfig>(context), IWillhabenConfigRepository
{
}
