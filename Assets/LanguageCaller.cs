using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageCaller : MonoBehaviour
{
    private void Start()
    {
        //Call the python script
        CallPythonScript();
    }
    //Make a function to call a python script on a local folder and run it, get the output and display it in the console
    public void CallPythonScript()
    {
        //Create a new process
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        //Set the process name to python
        process.StartInfo.FileName = "python";
        //Set the arguments to the path of the python script
        process.StartInfo.Arguments = Application.dataPath + "/Python/HelloWorld.py";
        //Set the working directory to the path of the python script
        process.StartInfo.WorkingDirectory = Application.dataPath;
        //Redirect the output
        process.StartInfo.RedirectStandardOutput = true;
        //Redirect the error
        process.StartInfo.RedirectStandardError = true;
        //Set the process to use the shell
        process.StartInfo.UseShellExecute = false;
        //Start the process
        process.Start();
        //Get the output
        string output = process.StandardOutput.ReadToEnd();
        //Get the error
        string error = process.StandardError.ReadToEnd();
        //Write the output to the console
        Debug.Log(output);
        //Write the error to the console
        Debug.Log(error);
        //Wait for the process to exit
        process.WaitForExit();
    }
}
