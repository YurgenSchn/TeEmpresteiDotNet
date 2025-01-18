# Te Emprestei (.NET e Angular)
Este é um projeto de estudos de .NET, ASP.NET e Angular. Aqui estou explorando API's RESTful no back-end em dotNet, e arriscando algum front-end com Angular. Pretendo recriar esta ideia em uma aplicação mobile.

##

### Ideia do Aplicativo
TeEmprestei é uma aplicação WEB para registrar pagamentos entre amigos, oferecendo resumos e informações contextualizadas.<br>
O objetivo é oferecer uma plataforma de fácil acesso para registrar e visualizar o dinheiro emprestado e devido entre amigos.

Quando um usuário "paga" para outro, o sistema calcula o valor emprestado e recebido entre cada amigo, gerando um saldo total e entre amigos, mantendo um histórico compartilhado.

![Current Front-Page](https://cdn.discordapp.com/attachments/1072630091529601106/1081319771976515666/image.png?ex=678c1603&is=678ac483&hm=63103e0953558a840dffb5f3662b0f1a0f6352da46022e933e0cf54473afc41a&)

👷‍♂️ Estado atual (03/03/23):

- É necessário criar um componente de lista de pagamento, que será reutilizado no resumo entre amigos (TODO).
- O saldo é calculado junto da lista de pagamentos. É preciso componentizar o saldo também.
- Melhorar sistema de roles, pois precisa pré-cadastradar no banco de dados para tudo funcionar bem ("usuario" e "admin" na tabela access_level)

##

### Entidades do Banco de Dados

#### User
![Usuario](https://cdn.discordapp.com/attachments/1072630091529601106/1081633168781754518/image.png?ex=678be863&is=678a96e3&hm=1105106c782daac5446b5ff113c4088f802b197e273f44c442668cc60d705fd4&)

Esta entidade armazena informações pessoais e de autenticação do usuário. As chaves pública e privada são compostas por 16 caracteres ASCII aleatórios, oferencendo uma maneira de validar requisições além do JWT Token: o id_privado só é fornecido ao logar. As senhas são armazenadas em hash com chave fixa (HMACSHA384).

##

#### Access Level
![Access Level](https://cdn.discordapp.com/attachments/1072630091529601106/1079855234173632622/Access_Level.png?ex=678c080e&is=678ab68e&hm=ca1a9ae3a8b25b6046c6f0c999ca8ea0d41f1d615050d89841ad6fbcec1935a1&)

Uma entidade para armazenar os níveis de acesso. Criada para otimizar espaço na tabela do usuário.

##

#### Friends
![Friends](https://cdn.discordapp.com/attachments/1072630091529601106/1079855234479837235/Friend.png?ex=678c080e&is=678ab68e&hm=afa824d2a1f1c1fcac80b694ae1c989d7d16e65c766e99a6964e886539352c4f&)

Esta entidade relaciona dois usuários, o "sender" e o "receiver", em uma relação de amizade. O sender é quem fez a solicitação da amizade, e o receiver será o outro amigo, que deverá confirmar a amizade.

##

#### Payments
![Payments](https://cdn.discordapp.com/attachments/1072630091529601106/1079855234681143408/Payment.png?ex=678c080e&is=678ab68e&hm=1f0a1f64d3e43ddca65813931b40bb101f12171b1ecd66ded500073635e23958&)

Esta entidade armazena pagamentos entre usuários. A validação no back-end só permite pagamentos entre usuários com amizade confirmada. Um pagamento possui apenas valor e descrição.
