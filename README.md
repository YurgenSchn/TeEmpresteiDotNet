# Te Emprestei (.NET e Angular)
Este é um projeto de estudos de .NET, ASP.NET e Angular. Aqui estou explorando API's RESTful no back-end em dotNet, e arriscando algum front-end com Angular. Pretendo recriar esta ideia em uma aplicação mobile.

##

### Ideia do Aplicativo
TeEmprestei é uma aplicação WEB para registrar pagamentos entre amigos, oferecendo resumos e informações contextualizadas.<br>
O objetivo é oferecer uma plataforma de fácil acesso para registrar e visualizar o dinheiro emprestado e devido entre amigos.

Quando um usuário "paga" para outro, o sistema calcula o valor emprestado e recebido entre cada amigo, gerando um saldo total e entre amigos, mantendo um histórico compartilhado.

![Current Front-Page](https://cdn.discordapp.com/attachments/1072630091529601106/1079858592183754903/Web_app.png)

👷‍♂️ Estado atual (01/03/23):

- O saldo é calculado no front-end, usando os pagamentos que já são enviados para a lista.
- Funções do back-end precisam ser assíncronas.
- Roles precisam ser pré-cadastrados no banco de dados ("usuario" e "admin" na tabela access_level)

##

### Entidades do Banco de Dados

#### User
![Usuario](https://cdn.discordapp.com/attachments/1072630091529601106/1079855233796165712/User.png)

Esta entidade armazena informações pessoais e de autenticação do usuário. As chaves pública e privada são compostas por 16 caracteres ASCII aleatórios, oferencendo uma maneira de validar requisições além do JWT Token. As senhas são armazenadas em hash, por HMACSHA384 (hash com chave fixa).

##

#### Access Level
![Access Level](https://cdn.discordapp.com/attachments/1072630091529601106/1079855234173632622/Access_Level.png)

Uma entidade para armazenar os níveis de acesso. Criada para otimizar espaço na tabela do usuário.

##

#### Friends
![Friends](https://cdn.discordapp.com/attachments/1072630091529601106/1079855234479837235/Friend.png)

Esta entidade relaciona dois usuários, o "sender" e o "receiver", em uma relação de amizade. O sender é quem fez a solicitação da amizade, e o receiver será o outro amigo, que deverá confirmar a amizade.

##

#### Payments
![Payments](https://cdn.discordapp.com/attachments/1072630091529601106/1079855234681143408/Payment.png)

Esta entidade armazena pagamentos entre usuários. A validação no back-end só permite pagamentos entre usuários com amizade confirmada. Um pagamento possui apenas valor e descrição.
