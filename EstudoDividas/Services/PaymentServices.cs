using EstudoDividas.Contracts;
using EstudoDividas.Data.MySQL.Entities;
using EstudoDividas.Data.MySQL;
using Microsoft.EntityFrameworkCore;
using EstudoDividas.Contracts.ReturnTypes;
using Org.BouncyCastle.Asn1.Ocsp;

namespace EstudoDividas.Services
{
    public class PaymentServices
    {
        // Construtor
        MySQLContext _context;
        public PaymentServices(MySQLContext context)
        {
            _context = context;
        }

        // Funções principais
        public PayFriendResponseContract payFriend(PayFriendRequestContract request)
        {

            // FILTRO = se o private-public ids do requerente não baterem
            var isValidRequester = _context.User.Where(u => u.id_private.Equals(request.userPrivateId) &&
                                                            u.id_public.Equals(request.userPublicId)).Any();
            if (!isValidRequester)
                return new()
                {
                    status = "bad_auth",
                    message = "Autenticação inválida"
                };

            // FILTRO = se os ids seu e do amigo forem iguais
            if (request.userPublicId == request.friendPublicId)
                return new()
                {
                    status = "invalid_self_pay",
                    message = "Não é possível pagar para si próprio."
                };

            // FILTRO = se id do amigo existe
            var friend_exists = _context.User.Where(u => u.id_public.Equals(request.friendPublicId)).Any();
            if (!friend_exists)
                return new()
                {
                    status = "inexistent_friend",
                    message = "Amigo inexistente"
                };

            // FILTRO = se tem amizade confirmada
            var isConfirmedFriend = _context.Friend.Where(f => f.confirmed == true &&
                                                               f.sender == request.userPublicId && f.receiver == request.friendPublicId ||
                                                               f.sender == request.friendPublicId && f.receiver == request.userPublicId).Any();

            if (!isConfirmedFriend)
                return new()
                {
                    status = "not_friends",
                    message = "Este usuário não é seu amigo."
                };

            // FILTRO = se o valor for inválido (de acordo com o banco de dados)
            if (request.value <= 0 || request.value > 9999999999)
                return new()
                {
                    status = "invalid_value",
                    message = "Valor inválido"
                };

            // FILTRO = Sem descrição
            if (request.description == null || request.description.Length == 0)
                return new()
                {
                    status = "no_payment_description",
                    message = "Sem descrição de pagamento"
                };

            // Adicionar Pagamento
            Payment payment = new()
            {
                description = request.description,
                value = request.value,
                sender = request.userPublicId,
                receiver = request.friendPublicId,
                sent_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                confirmed = false
            };

            _context.Payment.Add(payment);
            _context.SaveChanges();

            return new()
            {
                status = "ok",
                message = "Pagamento Realizado com Sucesso",
            };

        }

        public ConfirmPaymentResponseContract confirmPayment(ConfirmPaymentRequestContract request)
        {
            // VALIDAÇÕES QUE RETORNAM

            // FILTRO = se o private-public ids do requerente não baterem
            var isValidRequester = _context.User.Where(u => u.id_private.Equals(request.userPrivateId) &&
                                                            u.id_public.Equals(request.userPublicId)).Any();
            if (!isValidRequester)
                return new()
                {
                    status = "bad_auth",
                    message = "Autenticação inválida"
                };

            // FILTRO = Pagamento invalido de alguma forma: id não existe, já foi confirmado, ou quem recebeu foi outro usuario
            var payment = _context.Payment.Where(p =>  p.id.Equals(request.paymentId) &&
                                                       p.confirmed.Equals(false) &&
                                                       p.receiver.Equals(request.userPublicId)).FirstOrDefault();
            if (payment == null)
                return new()
                {
                    status = "invalid_payment",
                    message = "Pagamento Inválido"
                };

            // FILTRO = Checar se quem fez o pagamento é amigo (deve ser, só uma camada de segurança)
            var senderIsFriend = _context.Friend.Where(f => f.confirmed.Equals(true) &&
                                                            (f.sender.Equals(request.userPublicId) && f.receiver.Equals(payment.sender)) ||
                                                            (f.sender.Equals(payment.sender) && f.receiver.Equals(request.userPublicId))).Any();
            if (!senderIsFriend)
                return new()
                {
                    status = "fraudlent_payment",
                    message = "Pagamento realizado sem amizade confirmada. Checar com o administrador"
                };

            payment.confirmed = true;
            payment.confirmed_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _context.SaveChanges();

            return new()
            {
                status = "ok",
                message = "Pagamento confirmado com sucesso!"
            };

        }

        public GetPaymentHistoryResponseContract getPaymentHistory(string userPublicId, string userPrivateId, string friendPublicId = "")
        {
            // FILTRO = se o private-public ids do requerente não baterem
            var isValidRequester = _context.User.Where(u => u.id_private.Equals(userPrivateId) &&
                                                            u.id_public.Equals(userPublicId)).Any();
            if (!isValidRequester)
                return new()
                {
                    status = "bad_auth",
                    message = "Autenticação inválida"
                };

            // Checar se o id de amigo é valido
            if (friendPublicId != "")
            {
                var isFriend = _context.Friend.Where(f => f.confirmed.Equals(true))
                                              .Where(f => f.sender.Equals(friendPublicId) && f.receiver.Equals(userPublicId) ||
                                                          f.sender.Equals(userPublicId)   && f.receiver.Equals(friendPublicId)).Any();
                if (!isFriend) return new()
                {
                    status = "invalid_friend",
                    message = "Esse usuário não é seu amigo."
                };
            }

            var payments = (from p in _context.Payment
                            join us in _context.User on p.sender equals us.id_public
                            join ur in _context.User on p.receiver equals ur.id_public
                            orderby p.sent_date
                            where (p.sender == userPublicId) ||
                                  (p.receiver == userPublicId)
                            select new PaymentReturnType
                            {
                                id = p.id,
                                value = p.value,
                                description = p.description,
                                date = p.sent_date,
                                receiver_id = p.receiver,
                                receiver_name = ur.name,
                                sender_id = p.sender,
                                sender_name = us.name,
                                confirmed = p.confirmed
                            });

            if (friendPublicId != "")
            {
                payments = payments.Where(p => (p.sender_id.Equals(userPublicId)   && p.receiver_id.Equals(friendPublicId)) ||
                                                p.sender_id.Equals(friendPublicId) && p.receiver_id.Equals(userPublicId));
            }


            return new()
            {
                payments = payments.ToList(),
                status = "ok",
                message = "Histórico de pagamentos coletados com sucesso."
            };

        }
    }
}
