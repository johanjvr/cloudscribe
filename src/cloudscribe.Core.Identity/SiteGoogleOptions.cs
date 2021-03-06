﻿// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2017-07-27
// Last Modified:			2017-08-30
// 

using cloudscribe.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace cloudscribe.Core.Identity
{
    public class SiteGoogleOptions : IOptionsMonitor<GoogleOptions>
    {
        public SiteGoogleOptions(
            IOptionsFactory<GoogleOptions> factory,
            IEnumerable<IOptionsChangeTokenSource<GoogleOptions>> sources,
            IOptionsMonitorCache<GoogleOptions> cache,
            IOptions<MultiTenantOptions> multiTenantOptionsAccessor,
            IPostConfigureOptions<GoogleOptions> optionsInitializer,
            IDataProtectionProvider dataProtection,
            IHttpContextAccessor httpContextAccessor,
            ILogger<SiteGoogleOptions> logger
            )
        {
            _multiTenantOptions = multiTenantOptionsAccessor.Value;
            _httpContextAccessor = httpContextAccessor;
            _log = logger;
            _optionsInitializer = optionsInitializer;
            _dp = dataProtection;

            _factory = factory;
            _sources = sources;
            _cache = cache;

            foreach (var source in _sources)
            {
                ChangeToken.OnChange<string>(
                    () => source.GetChangeToken(),
                    (name) => InvokeChanged(name),
                    source.Name);
            }
        }

        private readonly IOptionsMonitorCache<GoogleOptions> _cache;
        private readonly IOptionsFactory<GoogleOptions> _factory;
        private readonly IEnumerable<IOptionsChangeTokenSource<GoogleOptions>> _sources;
        internal event Action<GoogleOptions, string> _onChange;

        private MultiTenantOptions _multiTenantOptions;
        private IHttpContextAccessor _httpContextAccessor;
        private ILogger _log;
        private IPostConfigureOptions<GoogleOptions> _optionsInitializer;
        private readonly IDataProtectionProvider _dp;

        private void InvokeChanged(string name)
        {
            name = name ?? Options.DefaultName;
            _cache.TryRemove(name);
            var options = Get(name);
            if (_onChange != null)
            {
                _onChange.Invoke(options, name);
            }
        }

        private GoogleOptions ResolveOptions(string scheme)
        {
            var tenant = _httpContextAccessor.HttpContext.GetTenant<SiteContext>();
            var options = new GoogleOptions();
            options.ClientId = "placeholder";
            options.ClientSecret = "placeholder";
            _optionsInitializer.PostConfigure(scheme, options);

            options.DataProtectionProvider = options.DataProtectionProvider ?? _dp;

            if (options.Backchannel == null)
            {
                options.Backchannel = new HttpClient(options.BackchannelHttpHandler ?? new HttpClientHandler());
                options.Backchannel.DefaultRequestHeaders.UserAgent.ParseAdd("Microsoft ASP.NET Core OAuth handler");
                options.Backchannel.Timeout = options.BackchannelTimeout;
                options.Backchannel.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB
            }

            if (options.StateDataFormat == null)
            {
                var dataProtector = options.DataProtectionProvider.CreateProtector(
                    typeof(OAuthHandler<GoogleOptions>).FullName, scheme, "v1");

                options.StateDataFormat = new PropertiesDataFormat(dataProtector);
            }

            ConfigureTenantOptions(tenant, options);

            return options;

        }

        private void ConfigureTenantOptions(SiteContext tenant, GoogleOptions options)
        {
            if (tenant == null)
            {
                _log.LogError("tenant was null");
                return;
            }
            var useFolder = !_multiTenantOptions.UseRelatedSitesMode
                                        && _multiTenantOptions.Mode == cloudscribe.Core.Models.MultiTenantMode.FolderName
                                        && tenant.SiteFolderName.Length > 0;

            if (!string.IsNullOrWhiteSpace(tenant.GoogleClientId))
            {
                options.ClientId = tenant.GoogleClientId;
                options.ClientSecret = tenant.GoogleClientSecret;
                options.SignInScheme = IdentityConstants.ExternalScheme;

                if (useFolder)
                {
                    options.CallbackPath = "/" + tenant.SiteFolderName + "/signin-google";
                }
            }
        }

        public GoogleOptions CurrentValue
        {
            get
            {
                return ResolveOptions(GoogleDefaults.AuthenticationScheme);
            }
        }

        public GoogleOptions Get(string name)
        {
            return ResolveOptions(name);
        }

        public IDisposable OnChange(Action<GoogleOptions, string> listener)
        {
            _log.LogDebug("onchange invoked");

            var disposable = new ChangeTrackerDisposable(this, listener);
            _onChange += disposable.OnChange;
            return disposable;
        }


        internal class ChangeTrackerDisposable : IDisposable
        {
            private readonly Action<GoogleOptions, string> _listener;
            private readonly SiteGoogleOptions _monitor;

            public ChangeTrackerDisposable(SiteGoogleOptions monitor, Action<GoogleOptions, string> listener)
            {
                _listener = listener;
                _monitor = monitor;
            }

            public void OnChange(GoogleOptions options, string name) => _listener.Invoke(options, name);

            public void Dispose() => _monitor._onChange -= OnChange;
        }

    }

    //public class SiteGoogleOptionsPreview : IOptionsSnapshot<GoogleOptions>
    //{
    //    public SiteGoogleOptionsPreview(
    //        IOptions<MultiTenantOptions> multiTenantOptionsAccessor,
    //        IPostConfigureOptions<GoogleOptions> optionsInitializer,
    //        IDataProtectionProvider dataProtection,
    //        IHttpContextAccessor httpContextAccessor,
    //        ILogger<SiteGoogleOptionsPreview> logger
    //        )
    //    {
    //        _multiTenantOptions = multiTenantOptionsAccessor.Value;
    //        _httpContextAccessor = httpContextAccessor;
    //        _log = logger;
    //        _optionsInitializer = optionsInitializer;
    //        _dp = dataProtection;
    //    }

    //    private MultiTenantOptions _multiTenantOptions;
    //    private IHttpContextAccessor _httpContextAccessor;
    //    private ILogger _log;
    //    private IPostConfigureOptions<GoogleOptions> _optionsInitializer;
    //    private readonly IDataProtectionProvider _dp;

    //    private GoogleOptions ResolveOptions(string scheme)
    //    {
    //        var tenant = _httpContextAccessor.HttpContext.GetTenant<SiteContext>();
    //        var options = new GoogleOptions();
    //        options.ClientId = "placeholder";
    //        options.ClientSecret = "placeholder";
    //        _optionsInitializer.PostConfigure(scheme, options);

    //        options.DataProtectionProvider = options.DataProtectionProvider ?? _dp;

    //        if (options.Backchannel == null)
    //        {
    //            options.Backchannel = new HttpClient(options.BackchannelHttpHandler ?? new HttpClientHandler());
    //            options.Backchannel.DefaultRequestHeaders.UserAgent.ParseAdd("Microsoft ASP.NET Core OAuth handler");
    //            options.Backchannel.Timeout = options.BackchannelTimeout;
    //            options.Backchannel.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB
    //        }

    //        if (options.StateDataFormat == null)
    //        {
    //            var dataProtector = options.DataProtectionProvider.CreateProtector(
    //                typeof(OAuthHandler<GoogleOptions>).FullName, scheme, "v1");

    //            options.StateDataFormat = new PropertiesDataFormat(dataProtector);
    //        }

    //        ConfigureTenantOptions(tenant, options);

    //        return options;

    //    }

    //    private void ConfigureTenantOptions(SiteContext tenant, GoogleOptions options)
    //    {
    //        if (tenant == null)
    //        {
    //            _log.LogError("tenant was null");
    //            return;
    //        }
    //        var useFolder = !_multiTenantOptions.UseRelatedSitesMode
    //                                    && _multiTenantOptions.Mode == cloudscribe.Core.Models.MultiTenantMode.FolderName
    //                                    && tenant.SiteFolderName.Length > 0;

    //        if (!string.IsNullOrWhiteSpace(tenant.GoogleClientId))
    //        {
    //            options.ClientId = tenant.GoogleClientId;
    //            options.ClientSecret = tenant.GoogleClientSecret;

    //            if (useFolder)
    //            {
    //                options.CallbackPath = "/" + tenant.SiteFolderName + "/signin-google";
    //            }
    //        }
    //    }

    //    public GoogleOptions Value
    //    {
    //        get
    //        {
    //            return ResolveOptions(GoogleDefaults.AuthenticationScheme);
    //        }
    //    }

    //    public GoogleOptions Get(string name)
    //    {
    //        return ResolveOptions(name);
    //    }

    //}
}
