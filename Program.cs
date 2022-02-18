#region Copyright (C) 2017-2022  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

//#define MUST_SEARCH
//If defined,     searches all *:\Program Files\WindowsApps\ subdirectories for wt.exe
//If not defined, uses the predefined (hardcoded) path

#region Using Directives

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;

#endregion

namespace CmdWt;
public class Program {

    public Program() {
        ProfileOptimization.SetProfileRoot(@"C:MyAppFolder");
        ProfileOptimization.StartProfile("Startup.Profile");
    }

    public static void Main() {
        AppDomain Current = AppDomain.CurrentDomain;
#if DEBUG
        string Args = TrimStart(Environment.CommandLine, $"\"{Current.BaseDirectory}{Current.FriendlyName}.dll\"");
#else
        string Args = TrimStart(Environment.CommandLine, $"\"{Current.BaseDirectory}{Current.FriendlyName}.exe\"");
#endif
#if MUST_SEARCH
        FileInfo? WtApp = FindWinApp("wt.exe");
        if ( WtApp is null ) {
            Console.WriteLine("wt.exe could not be found.");
            Environment.Exit(2);
            return;
        }
        Execute(WtApp.FullName, Args);
#else
        const string Path = "E:\\Programs\\Windows Terminal\\wt.exe";
        Execute(Path, Args);
#endif
        //_ = Console.ReadKey();
        //Environment.Exit(0);
    }

    /// <summary>
    /// Executes the specified application at the given path.
    /// </summary>
    /// <param name="Pth">The path.</param>
    /// <param name="Args">The arguments, or <see langword="null"/> or empty.</param>
    internal static void Execute(string Pth, string? Args) {
        FileInfo Exp = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe"));
        Process Proc = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = Exp.FullName,
                Arguments = $"{SmartQuote(Pth)} {Args}",
                UseShellExecute = false
            }
        };
        Console.WriteLine($"Running: {SmartQuote(Proc.StartInfo.FileName)} {Proc.StartInfo.Arguments}");
        _ = Proc.Start();
        _ = Console.ReadKey();
    }

    /// <summary>
    /// Appends double quotes (" ") to the given path if it contains a space character ( ).
    /// </summary>
    /// <param name="Path">The path.</param>
    /// <returns>A path with double quotes (if it contains a space), or the original string.</returns>
    internal static string SmartQuote( string Path ) => Path.Contains(' ') ? $"\"{Path}\"" : Path;

#if MUST_SEARCH

    /// <summary>
    /// Searches all drives for the given WindowsApp filename, and returns the found instance, or <see langword="null"/> if none are found.
    /// </summary>
    /// <param name="FN">The filename to search for.</param>
    /// <returns>The found <see cref="FileInfo"/> instance, or <see langword="null"/>.</returns>
    internal static FileInfo? FindWinApp( string FN ) {
        // ReSharper disable once LoopCanBePartlyConvertedToQuery
        foreach ( DriveInfo Drive in DriveInfo.GetDrives() ) {
            DirectoryInfo? WinAppDir = GetDir(Path.Combine(Drive.Name, "Program Files\\WindowsApps\\"), true);
            if ( WinAppDir is null ) { continue; }

            foreach ( DirectoryInfo Dir in WinAppDir.GetDirectories() ) {
                if ( FindFile(Dir, FN) is { } Fl ) {
                    return Fl;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Finds the file in the given directory, returning <see langword="null"/> if none are found.
    /// </summary>
    /// <param name="Dir">The directory to search.</param>
    /// <param name="FN">The filename to search for.</param>
    /// <returns>The found <see cref="FileInfo"/> instance, or <see langword="null"/>.</returns>
    internal static FileInfo? FindFile(DirectoryInfo Dir, string FN) {
        foreach ( FileInfo Fl in Dir.GetFiles(FN) ) {
            return Fl;
        }
        return null;
    }

    /// <summary>
    /// Gets the directory from the given string, returning <see langword="null"/> if the text was invalid, or <paramref name="MustExist"/> is <see langword="true"/> and the directory does not actually exist.
    /// </summary>
    /// <param name="Path">The path to convert to a <see cref="DirectoryInfo"/> instance.</param>
    /// <param name="MustExist">If <see langword="true" />, <see langword="null"/> is returned if the directory does not exist; otherwise the found instance will be removed regardless of whether a logical directory actually exists.</param>
    /// <returns>The found <see cref="DirectoryInfo"/> instance, or <see langword="null"/>.</returns>
    internal static DirectoryInfo? GetDir( string? Path, bool MustExist ) {
        if ( Path is null ) { return null; }
        try {
            DirectoryInfo Dir = new DirectoryInfo(Path);
            if ( MustExist && !Dir.Exists ) { return null; }
            return Dir;
        } catch {
            return null;
        }
    }

#endif

    /// <summary>
    /// Trims the start of the given string.
    /// </summary>
    /// <remarks>Unlike normal <see cref="string.TrimStart(char)"/> overloads, this version will only remove the beginning <b>once</b> at most.</remarks>
    /// <param name="S">The string to trim.</param>
    /// <param name="T">The text to remove from the start.</param>
    /// <returns>The trimmed string.</returns>
    [return: NotNullIfNotNull("S")]
    internal static string? TrimStart( string? S, string T ) => S is null ? null : S.StartsWith(T, StringComparison.OrdinalIgnoreCase) ? S[T.Length..] : S;

}