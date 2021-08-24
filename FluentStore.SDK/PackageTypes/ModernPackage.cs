﻿using FluentStore.SDK.Handlers;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Images;
using FluentStore.SDK.Messages;
using FluentStore.SDK.Models;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Marketplace.Storefront.Contracts.Enums;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Windows.Storage;
using Windows.System;
using Windows.System.Profile;
using ImageType = FluentStore.SDK.Images.ImageType;

namespace FluentStore.SDK.Packages
{
    public class ModernPackage<TModel> : PackageBase<TModel>
    {
        private Urn _Urn;
        public override Urn Urn
        {
            get
            {
                if (_Urn == null)
                    _Urn = Urn.Parse("urn:" + MicrosoftStoreHandler.NAMESPACE_MODERNPACK + ":" + PackageFamilyName);
                return _Urn;
            }
            set => _Urn = value;
        }

        public override bool Equals(PackageBase other)
        {
            if (other is ModernPackage<TModel> mpackage)
            {
                return mpackage.Type == this.Type && mpackage.PackageFamilyName == this.PackageFamilyName
                    && mpackage.Version == this.Version;
            }
            else
            {
                return base.Equals(other);
            }
        }

        public override bool RequiresDownloadForCompatCheck => true;
        public override async Task<string> GetCannotBeInstalledReason()
        {
            Guard.IsNotNull(DownloadItem, nameof(DownloadItem));
            return await PackagedInstallerHelper.GetCannotBeInstalledReason(
                (IStorageFile)DownloadItem, Type.HasFlag(InstallerType.Bundle));
        }

        public override async Task<bool> IsPackageInstalledAsync()
        {
            Guard.IsNotNull(PackageFamilyName, nameof(PackageFamilyName));
            try
            {
                return await PackagedInstallerHelper.IsInstalled(PackageFamilyName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking if {PackageFamilyName} is installed. Error:\r\n{ex}");
                return false;
            }
        }

        public override async Task<bool> InstallAsync()
        {
            // Make sure installer is downloaded
            Guard.IsEqualTo((int)Status, (int)PackageStatus.Downloaded, nameof(Status));

            if (await PackagedInstallerHelper.Install(this))
            {
                Status = PackageStatus.Installed;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override async Task LaunchAsync()
        {
            Guard.IsNotNull(PackageFamilyName, nameof(PackageFamilyName));
            await PackagedInstallerHelper.Launch(PackageFamilyName);
        }

        public override Task<IStorageItem> DownloadPackageAsync(StorageFolder folder = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines the <see cref="InstallerType"/> of the downloaded package.
        /// Requires <see cref="PackageBase.Status"/> to be <see cref="PackageStatus.Downloaded"/>.
        /// </summary>
        /// <returns>The file extension that corresponds with the determined <see cref="InstallerType"/>.</returns>
        public async Task<string> GetInstallerType()
        {
            Guard.IsEqualTo((int)Status, (int)PackageStatus.Downloaded, nameof(Status));
            Type = await PackagedInstallerHelper.GetInstallerType((StorageFile)DownloadItem);
            return Type.ToString().ToLower();
        }

        public override async Task<ImageBase> GetAppIcon()
        {
            Guard.IsNotNull(DownloadItem, nameof(DownloadItem));
            return await PackagedInstallerHelper.GetAppIcon(
                (StorageFile)DownloadItem, Type.HasFlag(InstallerType.Bundle));
        }

        public override async Task<ImageBase> GetHeroImage()
        {
            return null;
        }

        public override async Task<List<ImageBase>> GetScreenshots()
        {
            return new List<ImageBase>();
        }

        private InstallerType _Type;
        public InstallerType Type
        {
            get => _Type;
            set => SetProperty(ref _Type, value);
        }

        private string _PackageFamilyName;
        public string PackageFamilyName
        {
            get => _PackageFamilyName;
            set => SetProperty(ref _PackageFamilyName, value);
        }

        private string _PublisherDisplayName;
        public string PublisherDisplayName
        {
            get => _PublisherDisplayName;
            set => SetProperty(ref _PublisherDisplayName, value);
        }

        private string _LogoRelativePath;
        public string LogoRelativePath
        {
            get => _LogoRelativePath;
            set => SetProperty(ref _LogoRelativePath, value);
        }
    }
}
