﻿using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using RavenOverflow.Core.Entities;
using RavenOverflow.Services.Interfaces;

namespace RavenOverflow.Services
{
    public class UserService : IUserService
    {
        private readonly IDocumentSession _documentSession;

        public UserService(IDocumentSession documentSession)
        {
            _documentSession = documentSession;
        }

        #region IUserService Members

        public User CreateOrUpdate(OAuthData oAuthData, string userName, string fullName, string email)
        {
            // Lets find an existing user for the provider OR the email address if the provider doesn't exist.
            User user =
                _documentSession.Query<User>()
                    .SingleOrDefault(x => 
                        x.OAuthData.Any(y => y.Id == oAuthData.Id && y.OAuthProvider == oAuthData.OAuthProvider)) ??
                _documentSession.Query<User>().SingleOrDefault(x => x.Email == email);

            if (user != null)
            {
                // User exists, so lets update the OAuth data, for this user.
                if (user.OAuthData != null)
                {
                    OAuthData existingProvider =
                        user.OAuthData.SingleOrDefault(x => x.OAuthProvider == oAuthData.OAuthProvider);
                    if (existingProvider != null)
                    {
                        user.OAuthData.Remove(existingProvider);
                    }
                }
                else
                {
                    user.OAuthData = new List<OAuthData>();
                }

                user.OAuthData.Add(oAuthData);
            }
            else
            {
                // Ok. No user at all. We create one and store it.
                user = new User
                           {
                               DisplayName = userName,
                               Email = email,
                               Id = null,
                               FullName = fullName,
                               CreatedOn = DateTime.UtcNow,
                               IsActive = true,
                               OAuthData = new List<OAuthData>(),
                               FavoriteTags = new List<string> {"ravendb", "c#", "asp.net-mvc3"}
                           };
                user.OAuthData.Add(oAuthData);
            }

            _documentSession.Store(user);
            _documentSession.SaveChanges();

            return user;
        }

        #endregion
    }
}