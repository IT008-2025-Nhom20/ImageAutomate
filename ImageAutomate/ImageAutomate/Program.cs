using System.Diagnostics;
using ImageAutomate.Execution.Scheduling;
using ImageAutomate.Infrastructure;
using ImageAutomate.StandardBlocks;

namespace ImageAutomate
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            FormatRegistryInitializer.InitializeBuiltInFormats(ImageFormatRegistry.Instance);
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Main());
        }
    }
}