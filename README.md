# TeEmpresteiDotNet
Este é um projeto de estudos de .NET, ASP.NET e Angular. Aqui estou explorando API's RESTful no back-end em dotNet, e arriscando algum front-end com TypeScript e Angular.

##

### Sobre o Estudo
Do ponto de vista do estudo, busco aperfeiçoar conhecimentos brutos sobre o API's em dotNet, explorando arquiteturas, pacotes mais usados e boas práticas - além de explorar as ferramentas oferecidas pelo TypeScript e Angular no front-end, para prototipar/aperfeiçoar a ideia a nível de usabilidade.

Possuindo um protótipo com modelagem e usabilidade refinadas, pretendo recriar o aplicativo em uma stack amigável para mobile, talvez React, React Native e Nodejs.

##

### Ideia do Aplicativo
TeEmprestei é uma aplicação WEB para registrar pagamentos entre amigos, oferecendo resumos e informações contextualizadas.<br>
O objetivo é oferecer uma plataforma de fácil acesso para registrar e visualizar o dinheiro emprestado e devido entre amigos.

Quando um usuário "paga" para outro, o sistema calcula o valor emprestado e recebido entre cada amigo, gerando um saldo total e entre amigos, mantendo um histórico compartilhado.

![Current Front-Page](https://cdn.discordapp.com/attachments/1072630091529601106/1079858592183754903/Web_app.png)

*Sobre o estado atual (27/02/23):
O saldo é feito no front-end, com os pagamentos que já são enviados para a lista.
Ainda não montei o front para login. Portanto, o usuário está hardcoded no typescript para fazer o Http Request.
O endpoint do histórico está com a autenticação desabilitada (para o front).
Funções do back-end precisam ser assíncronas.

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
