// Code slightly modified from WJRRO's snippet
// https://discussions.unity.com/t/execute-shell-script-from-unity-script-osx-tts/491236/4
// Modifications by hsandt following doc:
// https://learn.microsoft.com/fr-fr/dotnet/api/system.diagnostics.process.standardoutput?view=net-8.0
// - using process and set its StartInfo directly
// - platform agnostic "bash" relying on presence in PATH
// - don't redirect Standard Input, and just pass a bash script + args as argument string to the main bash executable
// - ReadLine -> ReadToEnd and log the whole output string. Return void instead of IEnumerator (it wasn't returning lines anyway)

using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace HyperUnityCommons.Editor
{
    public static class EditorRunCommand
    {
        /// <summary>
        /// Run a bash command
        /// </summary>
        /// <param name="bashCommand">Bash command in the format "script.sh [arg1] [arg2]"</param>
        /// <param name="workingDirectory">Working directory to execute the command in</param>
        public static void RunCommand(string bashCommand, string workingDirectory)
        {
            using (Process process = new Process())
            {
                // Works for Unix bash as well as Windows custom bash, e.g. installed via scoop, as long as in PATH
                process.StartInfo.FileName = "bash";
                process.StartInfo.WorkingDirectory = workingDirectory;
                // the bash command is passed as argument to the main bash executable, along with its own arguments
                process.StartInfo.Arguments = bashCommand;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                // because of the indirection, we cannot easily check if bash script is missing,
                // we will just see an empty output log
                string output = process.StandardOutput.ReadToEnd();
                Debug.Log(output);

                process.WaitForExit();
            }
        }
    }
}
