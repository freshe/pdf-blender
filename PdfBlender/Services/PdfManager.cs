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

using iText.Kernel.Pdf;
using PdfBlender.Core;

namespace PdfBlender.Services;

public class PdfManager : IPdfManager
{
    public Stream MergePdfFiles(IPdfMergerOptions options, IEnumerable<Stream> files)
    {
        //output stream
        var stream = new MemoryStream();
        
        //dispose when done
        using var writer = new PdfWriter(stream);
        writer.SetCloseStream(false);
        writer.SetSmartMode(true);
        
        //the new "combined" document
        var document = new PdfDocument(writer);
        
        //producer wille be X; modified using iText under AGPL
        document.GetDocumentInfo().SetProducer("freshbit PDF-blender (AGPL)");
        
        if (options.Tagged)
        {
            document.InitializeOutlines();
            document.SetTagged();
            document.GetCatalog().SetViewerPreferences(new PdfViewerPreferences().SetDisplayDocTitle(true));
        }

        if (!string.IsNullOrEmpty(options.Language))
        {
            document.GetCatalog().SetLang(new PdfString(options.Language));
        }

        foreach (var file in files)
        {
            using var reader = new PdfReader(file);
            var currentDocument = new PdfDocument(reader);
            
            currentDocument.CopyPagesTo(1, currentDocument.GetNumberOfPages(), document);
            currentDocument.Close();
            
            file.Dispose();
        }

        document.Close();
        stream.Position = 0;

        return stream;
    }
}