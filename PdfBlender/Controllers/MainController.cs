﻿/*
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
    
    public MainController(IPdfManager pdfManager)
    {
        _pdfManager = pdfManager;
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
    public async Task<IActionResult> Upload(UploadViewModel viewModel, List<IFormFile> files)
    {
        if (ModelState.IsValid && files.Count > 0)
        {
            var streams = new List<Stream>();
            
            foreach (var file in files)
            {
                var temp = file.OpenReadStream();
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
            
            return File(pdf, "application/pdf", outputFileName);
        }
        
        throw new Exception("Upload error");
    }
}