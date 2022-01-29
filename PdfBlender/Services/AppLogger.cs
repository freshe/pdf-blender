/*
PDF Blender - A web based pdf merger powered by iText
Copyright (C) 2022 freshbit

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

namespace PdfBlender.Services;

public class AppLogger
{
    /*
     *  Todo.
     *  Oh dear, make a good efficient logger for god's sake!
     */

    private static readonly object LockObj = new object();
    private readonly string _logFilePath;

    public bool Log { get; }
    
    public AppLogger(bool log, string logFilePath)
    {
        Log = log;
        _logFilePath = logFilePath;
    }

    public void WriteToLog(string text)
    {
        lock (LockObj)
        {
            try
            {
                using var textWriter = new StreamWriter(_logFilePath, append: true);
                textWriter.WriteLine(text);
            }
            catch
            {
                //ignore
            }
        }
    }
}