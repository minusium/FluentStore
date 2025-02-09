﻿namespace FluentStore.SDK.Messages
{
    public class SuccessMessage
    {
        public SuccessMessage(string message, object context = null, SuccessType type = SuccessType.None)
        {
            Message = message;
            Context = context;
            Type = type;
        }

        public string Message { get; set; }
        public object Context { get; set; }
        public SuccessType Type { get; set; }

        public static SuccessMessage CreateForPackageDownloadCompleted(PackageBase package)
        {
            return new(package.Title + " is ready to install.", package, SuccessType.PackageDownloadCompleted);
        }

        public static SuccessMessage CreateForPackageInstallCompleted(PackageBase package)
        {
            return new(package.Title + " just got installed.", package, SuccessType.PackageInstallCompleted);
        }
    }

    public enum SuccessType
    {
        None,
        PackageFetchCompleted,
        PackageDownloadCompleted,
        PackageInstallCompleted,
    }
}
