using System.Runtime.CompilerServices;

namespace uOrgHub.Tests;

internal static class TestSetup
{
    /// <summary>QuestPDF requires a license declared once at process startup; the API does this in
    /// Program.cs, which test runs never execute, so it's declared here instead.</summary>
    [ModuleInitializer]
    internal static void Init()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }
}
