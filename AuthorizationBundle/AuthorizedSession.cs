using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Raven.Client.Documents;
using Raven.Client.Documents.Commands.Batches;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Session;
using Raven.Client.Exceptions;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace AuthorizationBundle
{
    public sealed class AuthorizedSession : IDisposable
    {
        private static IDocumentSession _session;

        private AuthorizedSession(IDocumentStore store, IDocumentSession session, User user)
        {
            _store = store;
            _session = session;
            _user = user;
        }

        public static AuthorizedSession OpenSession(IDocumentStore store , string userId, string password)
        {
            IDocumentSession session = null;
            try
            {
                session = store.OpenSession();                
                var user = session.Load<User>(userId);
                if (user == null)
                    ThrowNoPermissions(userId);
                //TODO:change to graph query
                if (CheckValidAuthorizedUser(userId, password, user.PasswordHash) == false)
                    ThrowNoPermissions(userId);                
                return new AuthorizedSession(store, session, user);
            }
            catch
            {
                session?.Dispose();
                throw;
            }
        }

        private static readonly string Root = "RootUser"; 
        public void CreateUser(string user, string password)
        {
            using(var session = _store.OpenSession(new SessionOptions
            {
                TransactionMode = TransactionMode.ClusterWide
            }))
            {
                var value = session.Advanced.ClusterTransaction.GetCompareExchangeValue<string>("users/" + Root);
                if (value.Value != _user.Id)
                {
                    throw new UnauthorizedAccessException($"User {_user.Id} is not allowed to create new users");
                }
                session.Advanced.ClusterTransaction.CreateCompareExchangeValue("users/"+user, true);
                session.Store(new User
                {
                    Id = user,
                    PasswordHash = ComputeHash(user, password)
                });
                session.SaveChanges();
            }
        }

        //TODO: add methods for giving and revoking permissions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ThrowNoPermissions(string userId)
        {
            throw new UnauthorizedAccessException($"User {userId} is not a registered user");
        }

        private static bool CheckValidAuthorizedUser(string user, string password, string userHash)
        {
            return userHash == ComputeHash(user, password);
        }

        private static string ComputeHash(string user, string password)
        {
            var sha = SHA256.Create();
            var userBuffer = Encoding.UTF8.GetBytes(user);
            var passwordBuffer = Encoding.UTF8.GetBytes(password);
            var buffer = new byte[userBuffer.Length + passwordBuffer.Length];
            Buffer.BlockCopy(userBuffer, 0, buffer, 0, user.Length);
            Buffer.BlockCopy(passwordBuffer, 0, buffer, user.Length, passwordBuffer.Length);
            var hash = Convert.ToBase64String(sha.ComputeHash(buffer));
            return hash;
        }

        private User _user;
        private IDocumentStore _store;

        public void Dispose()
        {
            _session?.Dispose();
        }

        public static void CreateRootUser(IDocumentStore store, string root, string password)
        {
            using (var session = store.OpenSession(new SessionOptions
            {
                TransactionMode = TransactionMode.ClusterWide
            }))
            {
                try
                {
                    session.Advanced.ClusterTransaction.CreateCompareExchangeValue("users/" + Root, root);
                    session.Advanced.ClusterTransaction.CreateCompareExchangeValue("users/" + root, true);
                    session.Store(new User
                    {
                        Id = root,
                        PasswordHash = ComputeHash(root, password)
                    });
                    session.SaveChanges();
                }
                catch (ConcurrencyException _)
                {
                    var rootName = session.Advanced.ClusterTransaction.GetCompareExchangeValue<string>("users/" + Root);
                    if (rootName.Value != root)
                    {
                        throw new UnauthorizedAccessException($"Can't generate root user {root} since there is another root user named {rootName.Value}");
                    }
                }
            }
        }

        public void CreateGroup(string groupName,string parent, string description, Permission permission)
        {
            using (var session = _store.OpenSession(new SessionOptions
            {
                TransactionMode = TransactionMode.ClusterWide
            }))
            {
                try
                {
                    session.Advanced.ClusterTransaction.CreateCompareExchangeValue("groups/" + groupName, true);
                    session.Store(new Group
                    {
                        Parent = parent,
                        Description = description,
                        Members = new List<string>(),
                        Permission = permission
                    }, groupName);
                    session.SaveChanges();
                }
                catch (ConcurrencyException _)
                {
                    //Group was already created 
                    var group = _session.Load<Group>(groupName);
                    if (group.Description != description)
                    {
                        throw new UnauthorizedAccessException($"Can't generate group {groupName} since such a group was already created with the following  description {group.Description}");
                    }
                }
            }
        }
    }
}
