# Microsoft Graph - ASP.NET MVC sample avec Microsoft Graph

## Pré-requis

Pour utiliser ce projet, il vous faut :
*  Visual Studio 2015 
* Un compte Office 365. Vous pouvez obtenir un [abonnement Office 365 Developer](https://portal.office.com/Signup/Signup.aspx?OfferId=6881A1CB-F4EB-4db3-9F18-388898DAF510&DL=DEVELOPERPACK&ali=1#0).
* Un tenant Azure pour y enregistrer votre application. Version d'essai : [Microsoft Azure](https://account.windowsazure.com/SignUp).

     > Important: vous devez relier votre souscription Azure à votre tenant Office 365 : [Creating and Managing Multiple Windows Azure Active Directories](http://blogs.technet.com/b/ad/archive/2013/11/08/creating-and-managing-multiple-windows-azure-active-directories.aspx).
* Enregistrer une application dans Azure AD pour obteenir un client, secret, redirect URI.

     > Note: Spécifiez **http://localhost:44035** comme **Sign-on URL**.  

## Configurer et exécuter l'application
1. Ouvrir **DemoGraph.sln**. 
2. Dans le Solution Explorer, ouvrir le fichier **Web.config**. 
3. Remplacer *APPID* avec l'ID client de votre application dans Azure.
4. Remplacer *APPSECRET* avec la clé de votre application dans Azure.
5. Remplacer *YOURTENANTID* avec l'ID de votre Azure AD **https://azure.microsoft.com/en-us/documentation/articles/resource-group-create-service-principal-portal/**.
6. Remplacer *[yourdomain]*.onmicrosoft.com par votre nom de domaine Office 365.
7. Puis lancez le debug (F5) et connectez-vous avec votre compte Office 365. Vérifiez bien que les packages Nuget sont correctement récupérés.

## Ressources supplémentaires

* [Microsoft Graph documentation](http://graph.microsoft.io)
* [Microsoft Graph API References](http://graph.microsoft.io/docs/api-reference/v1.0)
