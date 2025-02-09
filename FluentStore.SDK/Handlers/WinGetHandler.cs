﻿using FluentStore.SDK.Images;
using FluentStore.SDK.Packages;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WinGetRun;
using WinGetRun.Models;

namespace FluentStore.SDK.Handlers
{
    public class WinGetHandler : PackageHandlerBase
    {
        private readonly WinGetApi WinGetApi = Ioc.Default.GetRequiredService<WinGetApi>();

        public const string NAMESPACE_WINGET = "winget";
        public override HashSet<string> HandledNamespaces => new()
        {
            NAMESPACE_WINGET
        };

        public override string DisplayName => "WinGet";

        public override async Task<List<PackageBase>> GetFeaturedPackagesAsync()
        {
            var packages = new List<PackageBase>();
            var featured = await WinGetApi.GetFeatured();
            foreach (Package wgPackage in featured)
                packages.Add(new WinGetPackage(wgPackage));

            return packages;
        }

        public override async Task<PackageBase> GetPackage(Urn packageUrn)
        {
            Guard.IsEqualTo(packageUrn.NamespaceIdentifier, NAMESPACE_WINGET, nameof(packageUrn));

            var package = await WinGetApi.GetPackage(packageUrn.GetContent<NamespaceSpecificString>().UnEscapedValue);
            return new WinGetPackage(package);
        }

        public override async Task<List<PackageBase>> GetSearchSuggestionsAsync(string query)
        {
            var packages = new List<PackageBase>();
            var pageOptions = new PaginationOptions
            {
                Take = 3
            };
            var firstPage = await WinGetApi.SearchPackages(query: query, pageOptions: pageOptions);
            foreach (Package wgPackage in firstPage.Packages)
                packages.Add(new WinGetPackage(wgPackage));

            return packages;
        }

        public override async Task<List<PackageBase>> SearchAsync(string query)
        {
            var packages = new List<PackageBase>();
            var pageOptions = new PaginationOptions
            {
                Page = 0
            };

            for (int p = 0; p < 3; p++)
            {
                pageOptions.Page = p;
                var page = await WinGetApi.SearchPackages(query: query, pageOptions: pageOptions);
                foreach (Package wgPackage in page.Packages)
                    packages.Add(new WinGetPackage(wgPackage));
            }

            return packages;
        }

        public override ImageBase GetImage()
        {
            return new TextImage
            {
                Text = "\uE756",
                FontFamily = "Segoe MDL2 Assets"
            };
        }

        public override async Task<PackageBase> GetPackageFromUrl(Url url)
        {
            Regex rx = new(@"^https:\/\/((www\.)?github|raw\.githubusercontent)\.com\/microsoft\/winget-pkgs(\/(blob|tree))?\/master\/manifests\/[0-9a-z]\/(?<publisherId>[^\/\s]+)\/(?<packageId>[^\/\s]+)",
                RegexOptions.IgnoreCase);
            Match m = rx.Match(url.ToString());
            if (!m.Success)
                return null;

            return await GetPackage(Urn.Parse($"urn:{NAMESPACE_WINGET}:{m.Groups["publisherId"]}.{m.Groups["packageId"]}"));
        }

        public override Url GetUrlFromPackage(PackageBase package)
        {
            if (!(package is WinGetPackage wgPackage))
                throw new System.ArgumentException();

            char sortChar = wgPackage.PublisherId[0];
            string url = $"https://github.com/microsoft/winget-pkgs/tree/master/manifests/{sortChar}/{wgPackage.PublisherId}/{wgPackage.PublisherId}";

            if (wgPackage.Version != null)
                url += "/" + wgPackage.Version;

            return url;
        }
    }
}
