# oAuthProvider_Sample
Utilizando o ASOS para servir tokens oAuth (sem identity)

Para quem não quer utilizar o identity por diversos motivos esta é a forma mais fácil
que encontrei de gerar meus próprios JWT tokens e valida-los.

##Bibliotecas utilizadas
* AspNet.Security.OpenIdConnect.Server - Para criar os tokens
* AspNet.Security.OAuth.Validation - Para validar (pode utilizar esta para validar facebook, google etc)

##Explicação

Nas classes Startup.cs e AuthorizationProvider.cs coloquei alguns comentários para facilitar o entendimento

##Testes

Como é um projeto de conceito, preferi não criar os testes e colocar uma coleção do Postman para teste.
Basta importar o arquivo postman_collection.json no seu postman e executar os testes

BONUS: Note que no POST para criar o token, na aba TEST do Postman, eu coloquei um script para já pegar o token e colocar em uma variável de ambiente
Isto facilita muito para testar as apis, sem precisar ficar colando o token toda hora, basta fazer 1o um request e vc pode utilizar as outras apis.

##Links
Todos em inglês
* https://jwt.io/ - Para validar o token, verificar o que fica exposto e entender o que é o JWT
* https://github.com/aspnet-contrib/AspNet.Security.OpenIdConnect.Server - Git do ASOS (o responsável por criar o token)
* http://kevinchalet.com/2016/07/13/creating-your-own-openid-connect-server-with-asos-introduction/ - Artigo fenomenal para entender estas idas e voltas de token com todos os exemplos de integração

### Dúvidas ou sugestões
* Você pode criar um issue para discutirmos
* Você também pode criar um post no [ASP.NET Brasil](https://www.facebook.com/groups/aspnetmvcbr)