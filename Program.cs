using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace WslGitSelector {
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
            char[] cmd = Environment.CommandLine.ToCharArray();
            int length = Environment.GetCommandLineArgs()[0].Length;
            if (cmd[0] == '"') {
                length++;
            }
            if (cmd[length] == '"') {
                length++;
            }
            while (cmd[length] == ' ') {
                length++;
            }
            
            for (int i = length; i < cmd.Length - 8; i++) {
                if ((
                        (cmd[i] == ' ' || cmd[i] == '=' || cmd[i] == ':') && (cmd[i + 1] == '"' || cmd[i + 1] == '\'') ||
                        cmd[i + 1] == ' ' || cmd[i + 1] == '=' || cmd[i + 1] == ':'
                    ) &&
                    cmd[i + 2] == '/' && cmd[i + 3] == '/' &&
                    (cmd[i + 4] == 'W' || cmd[i + 4] == 'w') && 
                    (cmd[i + 5] == 'S' || cmd[i + 5] == 's') && 
                    (cmd[i + 6] == 'L' || cmd[i + 6] == 'l') && 
                    cmd[i + 7] == '$'
                    ) {

                    cmd[i + 2] = '\\';
                    cmd[i + 3] = '\\';
                }
            }

            procInfo.Arguments = new string(cmd, length, cmd.Length - length);
            procInfo.WorkingDirectory = workingDir;

            // プロセス実行
            Process p = Process.Start(procInfo);
            p.WaitForExit();

            return p.ExitCode;
        }
    }
}
