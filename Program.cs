using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace WslGitSwitcher {
    class Program {
        static int Main(string[] args) {
            char dirSep = Path.DirectorySeparatorChar;

            // 実行プロセスの情報を取得
            Process curProc = Process.GetCurrentProcess();
            string curProcDir = Path.GetDirectoryName(curProc.MainModule.FileName);

            // 作業フォルダ情報
            string workingDir = new string(Environment.CurrentDirectory);

            // settings.jsonファイル読み出し
            IConfigurationBuilder confBuilder = new ConfigurationBuilder();
            confBuilder.SetBasePath(curProcDir);
            confBuilder.AddJsonFile(@"settings.json");
            IConfigurationSection conf;

            ProcessStartInfo procInfo = new ProcessStartInfo();
            // 作業フォルダが \\wsl$ から始まる場合
            if (workingDir.StartsWith("\\\\wsl$\\", StringComparison.Ordinal)) {

                // wsl用設定を読み出し
                conf = confBuilder.Build().GetSection("wsl");
            } else {

                // デフォルト設定を読み出し
                conf = confBuilder.Build().GetSection("default");
            }

            // 相対パス解決
            Environment.CurrentDirectory = curProcDir;
            string execPath = Path.GetFullPath(conf[Path.GetFileNameWithoutExtension(curProc.MainModule.FileName)]);

            // 呼び出しプロセス情報設定
            procInfo.FileName = execPath;
            procInfo.Arguments = Environment.CommandLine.Remove(0, Environment.GetCommandLineArgs()[0].Length).TrimStart();
            procInfo.WorkingDirectory = workingDir;

            // プロセス実行
            Process p = Process.Start(procInfo);
            p.WaitForExit();

            return p.ExitCode;
        }
    }
}
