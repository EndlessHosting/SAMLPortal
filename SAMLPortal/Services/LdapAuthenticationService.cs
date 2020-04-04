﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;
using SAMLPortal.Models;

namespace SAMLPortal.Services
{
    public class LdapAuthenticationService : IAuthenticationService
    {
        private readonly LdapConnection _connection;

        public LdapAuthenticationService()
        {
            _connection = new LdapConnection
            {
                SecureSocketLayer = false
            };
        }

        public AppUser Login(string username, string password)
        {
            _connection.Connect(GlobalSettings.Get("LDAP_Host"), GlobalSettings.GetInt("LDAP_Port"));
            _connection.Bind(GlobalSettings.Get("LDAP_BindDN"), GlobalSettings.Get("LDAP_BindPass"));

            var adminSearchFilter = string.Format(GlobalSettings.Get("LDAP_AdminFilter"), username);
            var userSearchFilter = string.Format(GlobalSettings.Get("LDAP_UsersFilter"), username);

            List<string> filters = new List<string> { adminSearchFilter, userSearchFilter };
            bool hasResults = false;

            foreach (var filter in filters)
            {
                var searchResults = _connection.Search(
                    GlobalSettings.Get("LDAP_SearchBase"),
                    LdapConnection.ScopeSub,
                    filter,
                    new string[] { GlobalSettings.Get("LDAP_Attr_MemberOf"), GlobalSettings.Get("LDAP_Attr_DisplayName"), GlobalSettings.Get("LDAP_Attr_UID"), GlobalSettings.Get("LDAP_Attr_Mail") },
                    false
                );

                while (searchResults.HasMore())
                {
                    hasResults = true;
                    try
                    {
                        var user = searchResults.Next();
                        user.GetAttributeSet();
                        if (user != null)
                        {
                            _connection.Bind(user.Dn, password);
                            if (_connection.Bound)
                            {
                                var memberships = user.GetAttribute(GlobalSettings.Get("LDAP_Attr_MemberOf")).StringValueArray;

                                return new AppUser
                                {
                                    DisplayName = user.GetAttribute(GlobalSettings.Get("LDAP_Attr_DisplayName")).StringValue,
                                    Username = user.GetAttribute(GlobalSettings.Get("LDAP_Attr_UID")).StringValue,
                                    Email = user.GetAttribute(GlobalSettings.Get("LDAP_Attr_Mail")).StringValue,
                                    IsAdmin = filter == adminSearchFilter,
                                    Memberships = memberships
                                };
                            }
                        }

                        if (filter == userSearchFilter)
                        {
                            throw new Exception("Invalid username or password");
                        }
                    }
                    catch
                    {
                        throw new Exception("Invalid username or password");
                    }
                }

                if (hasResults)
                {
                    throw new Exception("Invalid username or password.");
                }
            }

            _connection.Disconnect();
            return null;
        }
    }
}