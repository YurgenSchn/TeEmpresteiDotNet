using EstudoDividas.Contracts.ReturnTypes;
using EstudoDividas.Data.MySQL.Entities;

namespace EstudoDividas.Contracts
{
    public class GetUserFriendsResponseContract : MinimalResponseContract
    {
        public List<FriendReturnType> friends { get; set; }
    }
}
