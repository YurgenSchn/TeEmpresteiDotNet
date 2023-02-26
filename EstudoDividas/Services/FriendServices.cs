using EstudoDividas.Contracts;
using EstudoDividas.Data.MySQL.Entities;
using EstudoDividas.Data.MySQL;
using Microsoft.EntityFrameworkCore;
using EstudoDividas.Contracts.ReturnTypes;

namespace EstudoDividas.Services
{
    public class FriendServices
    {
        // Construtor
        MySQLContext _context;
        public FriendServices(MySQLContext context)
        {
            _context = context;
        }

        // Funções principais
        public AddFriendResponseContract addFriend(AddFriendRequestContract request)
        {
            // Essa função gera um registro de amigo no banco
            // ou confirma um registro, caso já exista.

            // FILTRO = se os ids do Requerente são validos
            var isValidRequester = _context.User.Where(u => u.id_private.Equals(request.userPrivateId) &&
                                                            u.id_public.Equals(request.userPublicId)).Any();
            if (!isValidRequester)
                return new()
                {
                    status = "bad_auth",
                    message = "autenticação inválida"
                };

            // FILTRO = se os ids seu e do amigo forem iguais
            if (request.userPublicId == request.friendPublicId)
                return new()
                {
                    status = "invalid_self_add",
                    message = "Não é possível se adicionar como amigo"
                };

            // FILTRO = se id do amigo existe
            var friend_exists = _context.User.Where(u => u.id_public.Equals(request.friendPublicId)).Any();
            if (!friend_exists)
                return new()
                {
                    status = "inexistent_friend",
                    message = "Amigo inexistente"
                };

            // FILTRO = se o usuário já pediu amizade para esse amigo
            var already_requested = _context.Friend.Where(f =>  f.sender.Equals(request.userPublicId) &&
                                                                f.receiver.Equals(request.friendPublicId)).Any();
            if (already_requested)
                return new()
                {
                    status = "already_requested",
                    message = "Já solicitou amizade à esse usuário"
                };

            // FILTRO = se já é amigo confirmado
            var already_confirmed = _context.Friend.Where(f => f.confirmed.Equals(true))

                                                   .Where(f => (f.sender.   Equals(request.friendPublicId) &&
                                                                f.receiver. Equals(request.userPublicId)) ||

                                                               (f.sender.   Equals(request.userPublicId) &&
                                                                f.receiver. Equals(request.friendPublicId))).Any();
            if (already_confirmed)
                return new()
                {
                    status = "already_confirmed_friends",
                    message = "Já possui amizade confirmada com esse usuário"
                };


            // ADICIONAR OU ATUALIZAR REGISTRO DE AMIGO
            // Se o amigo solicitou antes, confirma amizade
            // Senão, cria o registro com confirmação pendente


            // Checar se o amigo já solicitou amizade antes
            var friend_requested = _context.Friend.Where(f =>   f.sender.Equals(request.friendPublicId) &&
                                                                f.receiver.Equals(request.userPublicId) &&
                                                                f.confirmed.Equals(false)).FirstOrDefault();
            if (friend_requested != null)
            {
                // Fazer um Update no registro existente de amizade
                friend_requested.confirmed_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                friend_requested.confirmed = true;

                _context.SaveChanges();

                return new()
                {
                    status = "ok",
                    message = "Amizade confimada com sucesso!"
                };

            }

            // Este usuário está fazendo o primeiro pedido, depois será confirmado.
            Friend friend = new()
            {
                sender = request.userPublicId,
                receiver = request.friendPublicId,
                confirmed = false
            };

            _context.Friend.Add(friend);
            _context.SaveChanges();

            return new()
            {
                status = "ok",
                message = "Solicitação de amigo realizada com sucesso!"
            };

        }

        public GetUserFriendsResponseContract getFriends(string userPublicId, string mode = "confirmed")
        {

            // Esta função consulta os amigos de um usuário
            // Pode consultar em 3 modos:
            //   'confirmed'        = amizades confirmadas (que ele recebeu ou enviou)
            //   'pending_sent'     = amizades pendentes (que ele solicitou)
            //   'pending_received' = amizades pendentes (que ele recebeu)

            //FILTRO DE USUARIO VAZIO
            if (userPublicId == "")
                return new()
                {
                    status = "empty_request",
                    message = "Pedido invalido, sem usuário ou modo"
                };

            // QUERY PARA AMIGOS CONFIRMADOS
            if (mode == "confirmed")
            {
                var friends =   // Amigos que receberam convite do usuário: da tabela de amigos, os RECEIVERS
                            (from f in _context.Friend
                             join u_receiver in _context.User on f.receiver equals u_receiver.id_public
                             where (f.sender == userPublicId) &&
                                   (f.confirmed == true)
                             select new FriendReturnType
                             {
                                 userPublicId = f.receiver,
                                 name = u_receiver.name,
                                 friendshipDate = f.confirmed_date
                             })

                            // Amigos que convidaram o usuário: da tabela de amigos, os SENDERS

                            .Concat(
                            (from f in _context.Friend
                             join u_sender in _context.User on f.sender equals u_sender.id_public
                             where (f.receiver == userPublicId) &&
                                   (f.confirmed == true)
                             select new FriendReturnType
                             {
                                 userPublicId = f.sender,
                                 name = u_sender.name,
                                 friendshipDate = f.confirmed_date
                             })
                            ).ToList();

                return new()
                {
                    status = "ok",
                    message = $"Amigos confirmados do usuário '{userPublicId}'.",
                    friends = friends
                };
            }

            // QUERY PARA AMIGOS PENDENTES SENT
            if (mode == "pending_sent")
            {
                var friends =   // Amigos que receberam convite do usuário (sem confirmar): da tabela de amigos, os RECEIVERS
                            (from f in _context.Friend
                             join u_receiver in _context.User on f.receiver equals u_receiver.id_public
                             where (f.sender == userPublicId) &&
                                   (f.confirmed == false)
                             select new FriendReturnType
                             {
                                 userPublicId = f.receiver,
                                 name = u_receiver.name,
                                 friendshipDate = f.confirmed_date
                             }).ToList();

                return new()
                {
                    status = "ok",
                    message = $"Amigos que ainda precisam aceitar confirmação do usuário '{userPublicId}'.",
                    friends = friends
                };
            }

            // QUERY PARA AMIGOS PENDENTES RECEIVED
            if (mode == "pending_received")
            {
                var friends =
                            // Amigos que convidaram o usuário (mas ele nao confirmou): da tabela de amigos, os SENDERS
                            (from f in _context.Friend
                             join u_sender in _context.User on f.sender equals u_sender.id_public
                             where (f.receiver == userPublicId) &&
                                   (f.confirmed == false)
                             select new FriendReturnType
                             {
                                 userPublicId = f.sender,
                                 name = u_sender.name,
                                 friendshipDate = f.confirmed_date
                             }).ToList();

                return new()
                {
                    status = "ok",
                    message = $"Amigos esperando confirmação do usuário '{userPublicId}'.",
                    friends = friends
                };
            }

            // FILTRO DE MODO INVÁLIDO
            return new()
            {
                status = "invalid_mode",
                message = "Modo inválido. Escolha entre 'confirmed', 'pending_sent' ou 'pending_received'."
            };
        }
    }
}
