using EstudoDividas.Contracts;
using EstudoDividas.Data.MySQL.Entities;
using EstudoDividas.Data.MySQL;
using Microsoft.EntityFrameworkCore;
using EstudoDividas.Contracts.ReturnTypes;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Globalization;

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
        public async Task<PayFriendResponseContract> payFriend(PayFriendRequestContract request)
        {

            // Um amigo paga o outro, definindo um valor e descrição

            // FILTROS QUE RETORNAM REQUEST INVÁLIDO
            //   a. Tentando pagar para si mesmo
            //   b. Valor ou descrição inválidas
            //   c. Ids do Requerente são inválidos
            //   d. Id do amigo não existe
            //   e. não possui amizade confirmada


            if (request.userPublicId == request.friendPublicId)
                return new()
                {
                    status = "invalid_self_pay",
                    message = "Não é possível pagar para si próprio."
                };

            if (request.value <= 0 || request.value > 9999999999)
                return new()
                {
                    status = "invalid_value",
                    message = "Valor inválido"
                };

            if (request.description == null || request.description.Length == 0)
                return new()
                {
                    status = "no_payment_description",
                    message = "Sem descrição de pagamento"
                };

            //QUERIES ASSÍNCRONAS
            var isValidRequester  = _context.User.Where(u => u.id_private.Equals(request.userPrivateId) &&
                                                            u.id_public.Equals(request.userPublicId)).AnyAsync();

            var friend_exists     = _context.User.Where(u => u.id_public.Equals(request.friendPublicId)).AnyAsync();

            var isConfirmedFriend = _context.Friend.Where(f => f.confirmed == true &&
                                                               f.sender == request.userPublicId && f.receiver == request.friendPublicId ||
                                                               f.sender == request.friendPublicId && f.receiver == request.userPublicId).AnyAsync();


            // RETORNOS AWAIT
            if (!await isValidRequester)
                return new()
                {
                    status = "bad_auth",
                    message = "Autenticação inválida"
                };

            if (! await friend_exists)
                return new()
                {
                    status = "inexistent_friend",
                    message = "Amigo inexistente"
                };

            if (! await isConfirmedFriend)
                return new()
                {
                    status = "not_friends",
                    message = "Este usuário não é seu amigo."
                };

            // ADICIONAR PAGAMENTO
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
            _context.SaveChangesAsync();

            return new()
            {
                status = "ok",
                message = "Pagamento Realizado com Sucesso",
            };

        }

        public async Task<ConfirmPaymentResponseContract> confirmPayment(ConfirmPaymentRequestContract request)
        {
            // Usuário referencia um pagamento que recebeu para confirmar

            // VALIDAÇÕES QUE RETORNAM
            //   - Ids do Requerente não batem.
            //   - Pagamento inválido: id não existe, já foi confirmado, ou quem recebeu foi outro usuario
            //   - Se quem fez o pagamento não é amigo (deve ser, mas só por precaução)

            var isValidRequester = _context.User.Where(u => u.id_private.Equals(request.userPrivateId) &&
                                                            u.id_public.Equals(request.userPublicId)).AnyAsync();

            if (!await isValidRequester)
                return new()
                {
                    status = "bad_auth",
                    message = "Autenticação inválida"
                };


            var payment = await _context.Payment.Where(p => p.id.Equals(request.paymentId) &&
                                                       p.confirmed.Equals(false) &&
                                                       p.receiver.Equals(request.userPublicId)).FirstOrDefaultAsync();

            if (payment == null)
                return new()
                {
                    status = "invalid_payment",
                    message = "Pagamento Inválido"
                };


            var senderIsFriend = _context.Friend.Where(f => f.confirmed.Equals(true) &&
                                                (f.sender.Equals(request.userPublicId) && f.receiver.Equals(payment.sender)) ||
                                                (f.sender.Equals(payment.sender) && f.receiver.Equals(request.userPublicId))).AnyAsync();

            if (! await senderIsFriend)
                return new()
                {
                    status = "fraudlent_payment",
                    message = "Pagamento realizado sem amizade confirmada. Checar com o administrador"
                };


            // CONFIRMAR O PAGAMENTO

            payment.confirmed = true;
            payment.confirmed_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _context.SaveChangesAsync();

            return new()
            {
                status = "ok",
                message = "Pagamento confirmado com sucesso!"
            };

        }

        public async Task<GetPaymentHistoryResponseContract> getPaymentHistory(string userPublicId, string userPrivateId, string friendPublicId = "")
        {
            // FILTRO = se o private-public ids do requerente não baterem
            var isValidRequester = await _context.User.Where(u => u.id_private.Equals(userPrivateId) &&
                                                            u.id_public.Equals(userPublicId)).AnyAsync();
            if (!isValidRequester)
                return new()
                {
                    status = "bad_auth",
                    message = "Autenticação inválida"
                };

            
            if (friendPublicId != "")
            {
                // Caso seja um request de histórico com um amigo, verificar se o ID do amigo é valido
                var isFriend = await _context.Friend.Where(f => f.confirmed.Equals(true))
                                              .Where(f => f.sender.Equals(friendPublicId) && f.receiver.Equals(userPublicId) ||
                                                          f.sender.Equals(userPublicId)   && f.receiver.Equals(friendPublicId)).AnyAsync();
                if (!isFriend) return new()
                {
                    status = "invalid_friend",
                    message = "Esse usuário não é seu amigo."
                };
            }


            // OBTER HISTÓRICO DE PAGAMENTO (mas não formar lista em memória ainda)
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

            //  FILTRAR POR AMIGO, SE TIVER O PARAMETRO
            if (friendPublicId != "")
            {
                payments = payments.Where(p => (p.sender_id.Equals(userPublicId)   && p.receiver_id.Equals(friendPublicId)) ||
                                                p.sender_id.Equals(friendPublicId) && p.receiver_id.Equals(userPublicId));
            }


            return new()
            {
                payments = payments.ToList(), // Criar lista na memória.
                status = "ok",
                message = "Histórico de pagamentos coletados com sucesso."
            };

        }
    }
}
