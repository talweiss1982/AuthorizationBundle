using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace AuthorizationBundle
{
    public sealed class SecuredSession : IDisposable
    {
        private static IDocumentSession _session;

        private SecuredSession(IDocumentSession session)
        {

        }

        public static SecuredSession OpenSession(DocumentStore store, string databaseName, string userId, string password)
        {
            if (databaseName == UsersDatabase)
                throw new UnauthorizedAccessException("You can't access the users database directly please use designated API methods");
            using (var userSession = store.OpenSession(UsersDatabase))
            {
                var user = userSession.Load<User>(userId);
                if(user == null)
                    throw new UnauthorizedAccessException($"User {userId} doesn't have permissions to access {databaseName} database");

            }
                var session = store.OpenSession(databaseName);
            return new SecuredSession(session);
        }

        private static void CheckValidAuthorizedUser(string user, string password)
        {
            var sha = SHA256.Create();

        }

        private readonly static string UsersDatabase = "Users";
        public void Dispose()
        {
        }
    }
}
