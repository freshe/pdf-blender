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

using Microsoft.AspNetCore.Mvc;
using PdfBlender.Core;
using PdfBlender.Services;
using PdfBlender.ViewModels;

namespace PdfBlender.Controllers;

public class MainController : Controller
{
    private readonly IPdfManager _pdfManager;
    private readonly AppLogger _appLogger;

    class PdfResult
    {
        public PdfResult(Stream pdf, string outputFileName)
        {
            Pdf = pdf;
            OutputFileName = outputFileName;
        }

        public Stream Pdf { get; }
        public string OutputFileName { get; }
    }
    
    public MainController(IPdfManager pdfManager, AppLogger appLogger)
    {
        _pdfManager = pdfManager;
        _appLogger = appLogger;
    }

    public IActionResult Index()
    {
        var model = new UploadViewModel
        {
            Tagged = false,
            FileName = "output.pdf",
            Language = "en"
        };
        
        return View(model);
    }

    [HttpPost]
    public IActionResult GetPdf(UploadViewModel viewModel, List<IFormFile> files)
    {
        if (ModelState.IsValid && files.Count > 0)
        {
            var result = GetResult(viewModel, files);
            return File(result.Pdf, "application/pdf", result.OutputFileName);
        }

        return new EmptyResult();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upload(UploadViewModel viewModel, List<IFormFile> files)
    {
        if (ModelState.IsValid && files.Count > 0)
        {
            var result = GetResult(viewModel, files);
            return File(result.Pdf, "application/pdf", result.OutputFileName);
        }
        
        throw new Exception("Upload error");
    }
    
    private PdfResult GetResult(UploadViewModel viewModel, List<IFormFile> files)
    {
        var streams = new List<Stream>();
        var names = new List<string>();
            
        foreach (var file in files)
        {
            var temp = file.OpenReadStream();
                
            names.Add(file.FileName);
            streams.Add(temp);
        }

        var pdf = _pdfManager.MergePdfFiles(new PdfMergerOptions
        {
            Language = viewModel.Language,
            Tagged = viewModel.Tagged
        }, streams);

        var outputFileName = viewModel.FileName;
        if (string.IsNullOrEmpty(outputFileName))
        {
            outputFileName = "default.pdf";
        }
            
        if (_appLogger.Log)
        {
            //Log as little as possible
            var inputNames = "[" + string.Join(",", names) + "]";
            var logLine = DateTime.UtcNow + " " + inputNames + " " + outputFileName;
                
            _appLogger.WriteToLog(logLine);
        }

        return new PdfResult(pdf, outputFileName);
    }
}