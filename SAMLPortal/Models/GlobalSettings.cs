﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Extensions.Configuration;
using SAMLPortal.Misc;

namespace SAMLPortal.Models
{
    public static class GlobalSettings
    {
        private static Dictionary<string, string> _appSettings = new Dictionary<string, string>();
        public static X509Certificate2 _signingCertificate;

        /// <summary>
        /// All environment variables for this app MUST begin with SP_
        /// </summary>
        public static void InitSettingsFromEnvironment()
        {
            DotNetEnv.Env.Load("./.env");
            IDictionary environment = Environment.GetEnvironmentVariables();

            if (Environment.GetEnvironmentVariable("SP_FORCE_ENV_REPOPUL") == "1")
            {
                // Re-fill databse with environment configuration
                foreach (var key in environment.Keys)
                {
                    if (key.ToString().StartsWith("SP_") && !key.ToString().StartsWith("SP_MYSQL"))
                    {
                        string shortenedKey = key.ToString().Remove(0, 3);
                        _appSettings.Add(shortenedKey, environment[key].ToString());
                    }
                }

                UpdateDatabaseSettings();
            }
            else
            {
                // Store database configuration in global settings
                UpdateFromDatabase();
            }

            
        }

        public static void GenerateSigningCertificate()
        {
            if (GlobalSettings.Get("SAML_Signing_Certificate") == null)
            {
                _signingCertificate = Helpers.GenerateCertificate(GlobalSettings.Get("CONFIG_CompanySubject"), GlobalSettings.Get("CONFIG_CompanyName"), GlobalSettings.Get("CONFIG_CompanyCountryCode"));

                GlobalSettings.Store("SAML_Signing_Certificate", Helpers.X509Certificate2ToString(_signingCertificate));
            }
            else
            {
                _signingCertificate = Helpers.StringToX509Certificate2(GlobalSettings.Get("SAML_Signing_Certificate"));
                Console.WriteLine(_signingCertificate.Thumbprint);
            }
        }

        /// <summary>
        /// Store a configuration inside the database and global settings
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Store(string key, string value)
        {
            _appSettings[key] = value;

            UpdateDatabaseSettings();
        }

        public static void UpdateDatabaseSettings()
        {
            SAMLPortalContext context = new SAMLPortalContext();

            foreach (var key in _appSettings.Keys)
            {
                var settingExists = context.KeyValue.Any(k => k.Key == key.ToString());

                if (settingExists)
                {
                    KeyValue settingToFind = context.KeyValue.Single(k => k.Key == key.ToString());
                    settingToFind.Value = _appSettings[key].ToString();
                }
                else
                {
                    KeyValue newSetting = new KeyValue()
                    {
                        Key = key,
                        Value = _appSettings[key]
                    };
                    context.KeyValue.Add(newSetting);
                }
            }

            context.SaveChanges();
        }

        public static void UpdateFromDatabase()
        {
            SAMLPortalContext context = new SAMLPortalContext();
            var settings = context.KeyValue.ToList();

            _appSettings.Clear();
            foreach (var setting in settings)
            {
                _appSettings[setting.Key] = setting.Value;
            }
        }

        public static string Get(string key)
        {
            try
            {
                return _appSettings[key];
            }
            catch (KeyNotFoundException ex)
            {
                return null;
            }
            
        }

        public static int GetInt(string key)
        {
            try
            {
                return Convert.ToInt32(_appSettings[key]);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while converting value : " + ex.Message);
            }
        }
    }
}