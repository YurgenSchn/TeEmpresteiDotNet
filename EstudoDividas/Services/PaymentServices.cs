using EstudoDividas.Contracts;
using EstudoDividas.Data.MySQL.Entities;
using EstudoDividas.Data.MySQL;
using Microsoft.EntityFrameworkCore;
using EstudoDividas.Contracts.ReturnTypes;

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
            // VALIDAÇÕES QUE RETORNAM
            // - Valor negativo ou superior ao limite do banco
            // - IDs do usuário requerente não batem
            // - ID do amigo não existe
            // - Não possui amizade confirmada com o amigo


            // FILTRO = se o private-public ids do requerente não baterem
            var isValidRequester = _context.User.Where(u => u.id_private.Equals(request.userPrivateId) && u.id_public.Equals(request.userPublicId)).FirstOrDefault();
            if (isValidRequester == null)
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
                    message = "Não é possível para para si próprio"
                };

            // FILTRO = se id do amigo existe
            var friend_exists = _context.User.FromSql($"SELECT * FROM user WHERE id_public = {request.friendPublicId}").Any();
            if (!friend_exists)
                return new()
                {
                    status = "inexistent_friend",
                    message = "Amigo inexistente"
                };

            // FILTRO = se tem amizade confirmada
            var isConfirmedFriend = _context.Friend.Where(f => f.confirmed == true &&
                                                               f.sender == request.userPublicId && f.receiver == request.friendPublicId ||
                                                               f.sender == request.friendPublicId && f.receiver == request.userPublicId).FirstOrDefault();

            if (isConfirmedFriend == null)
                return new()
                {
                    status = "not_friends",
                    message = "Este usuário não é seu amigo."
                };

            // FILTRO = se o valor for inválido
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
            // - Não existe usuário com os ids public e private do requerente
            // - O pagamento pendente não existe
            // - Quem recebeu o pagamento não é o Requerente

            // FILTRO = se o private-public ids do requerente não baterem
            var user = _context.User.Where(u => u.id_private.Equals(request.userPrivateId) && u.id_public.Equals(request.userPublicId)).FirstOrDefault();
            if (user == null)
                return new()
                {
                    status = "bad_auth",
                    message = "Autenticação inválida"
                };

            // FILTRO = Pagamento não existe: id invalido, já foi confirmado, ou quem recebeu foi outro usuario
            var payment = _context.Payment.Where(p => p.id.Equals(request.paymentId) &&
                                                      p.confirmed.Equals(false) &&
                                                      p.receiver.Equals(request.userPublicId)).FirstOrDefault();
            if (payment == null)
                return new()
                {
                    status = "invalid_payment",
                    message = "Pagamento Inválido"
                };

            // FILTRO = Checar se quem fez o pagamento é amigo (deve ser, só uma camada de segurança)
            var senderIsFriend = _context.Friend.Where(f => f.confirmed == true &&
                                                            (f.sender == request.userPublicId && f.receiver == payment.sender) ||
                                                            (f.sender == payment.sender && f.receiver == request.userPublicId)).Any();
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

        public GetPaymentHistoryResponseContract getPaymentHistory(string userPublicId, string friendPublicId = "")
        {
            if (userPublicId == null) return new()
                {
                    status = "no_user",
                    message = "Sem usuário"
                };

            if (friendPublicId != "")
            {
                var isFriend = _context.Friend.Where(f =>  (f.sender.Equals(friendPublicId) &&
                                                            f.receiver.Equals(userPublicId) &&
                                                            f.confirmed.Equals(true)) ||

                                                           (f.sender.Equals(userPublicId) &&
                                                            f.receiver.Equals(friendPublicId) &&
                                                            f.confirmed.Equals(true))
                                                            ).Any();
                if (!isFriend) return new()
                {
                    status = "invalid_friend",
                    message = "Esse usuário não é seu amigo."
                };
            }

            var payments = (from p in _context.Payment
                            where (p.sender == userPublicId) ||
                                  (p.receiver == userPublicId)
                            select new PaymentReturnType
                            {
                                value = p.value,
                                description = p.description,
                                date = p.sent_date,
                                receiver = p.receiver,
                                sender = p.sender,
                                confirmed = p.confirmed
                            });

            if (friendPublicId != "")
            {
                payments = payments.Where(p => (p.sender == userPublicId && p.receiver == friendPublicId) ||
                                                p.sender == friendPublicId && p.receiver == userPublicId);
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
