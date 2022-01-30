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

var FileDropValidationError;

(function (FileDropValidationError) {
    FileDropValidationError[FileDropValidationError["NoFiles"] = 0] = "NoFiles";
    FileDropValidationError[FileDropValidationError["TooManyFiles"] = 1] = "TooManyFiles";
    FileDropValidationError[FileDropValidationError["TooLargeFile"] = 2] = "TooLargeFile";
    FileDropValidationError[FileDropValidationError["InvalidFileType"] = 3] = "InvalidFileType";
})(FileDropValidationError || (FileDropValidationError = {}));

var FileDrop = (function () {
    function FileDrop(inputElement, options) {
        this._maxFileSize = 50;
        this._maxFileCount = 5;
        this._url = '';
        this._inputName = 'file';
        this.element = inputElement;
        this.options = options;
        
        /* silence analyzers etc */
        this.over = null;
        this.leave = null;
        this.start = null;
        this.progress = null;
        this.error = null;
        this.complete = null;
        this.validationError = null;
        
        if (!this.element) {
            console.log('Error: Invalid element selector');
            return;
        }
        
        this.initOptions();
        this.setupListeners();
    }
    
    FileDrop.prototype.getMaxFileSize = function () {
        return this._maxFileSize;
    };
    
    FileDrop.prototype.getMaxFileCount = function () {
        return this._maxFileCount;
    };
    
    FileDrop.prototype.handleDrop = function () {
        const validDrop = this.isValidDrop();
        
        if (!validDrop) {
            return;
        }
        
        const _this = this;
        const data = new FormData();
        this._xhr = new XMLHttpRequest();
        var expectedLength = 0
        var i = 0;
        
        if (this.options.form) {
            for (i = 0; i < this.options.form.elements.length; i++) {
                const element = this.options.form.elements[i];
                if (element.name && element.value) {
                    if (element.type === 'checkbox') {
                        if (element.checked) {
                            data.append(element.name, element.value);
                        }
                    } else {
                        data.append(element.name, element.value);
                    }
                }
            }
        }
        
        for (i = 0; i < this._files.length; i++) {
            const file = this._files[i];
            expectedLength += file.size;
            data.append(this._inputName, file);
        }
        
        this._xhr.responseType = 'blob';
        this._xhr.open('POST', this._url, true);
        
        this._xhr.addEventListener('readystatechange', function (e) {
            if (_this._xhr.readyState === 4) {
                if (_this._xhr.status === 200) {
                    const contentType = this.getResponseHeader('Content-Type');
                    const disposition = this.getResponseHeader('Content-Disposition');
                    var fileName = 'default';

                    if (disposition) {
                        const pattern = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
                        const matches = disposition.match(pattern);
                        if (matches && matches[1]) {
                            fileName = matches[1].replace(/['"]/g, '');
                        }
                    }

                    const blob = new Blob([this.response],{ type: contentType });
                    const a = document.createElement('a');
                    const url = window.URL.createObjectURL(blob);
                    
                    a.href = url;
                    a.download = fileName;
                    a.click();
                  
                    window.URL.revokeObjectURL(url);
                    
                    if (_this.complete) {
                        _this.complete();
                    }
                } else {
                    if (_this.error) {
                        _this.error();
                    }
                }
            }
        });
        
        if (this.progress) {
            this._xhr.upload.addEventListener('progress', function (e) {
                const temp = expectedLength > 0 ? 100.0 * e.loaded / expectedLength : 0;
                const percentage = Math.floor(temp);
                _this.progress(e.loaded, percentage);
            });
        }

        if (this.error) {
            this._xhr.addEventListener('abort', function (e) {
                _this.error();
            });
        }

        if (this.start) {
            this.start();
        }
        
        this._xhr.send(data);
    };
    
    FileDrop.prototype.cancel = function () {
        if (this._xhr) {
            this._xhr.abort();
        }
    };
    
    FileDrop.prototype.isValidDrop = function () {
        if (!this._files || !this._files.length || this._files.length === 0) {
            if (this.validationError) {
                this.validationError(FileDropValidationError.NoFiles);
            }
            
            return false;
        }
        
        if (this._files.length > this._maxFileCount) {
            if (this.validationError) {
                this.validationError(FileDropValidationError.TooManyFiles);
            }
            
            return false;
        }
        
        for (var i = 0; i < this._files.length; i++) {
            const file = this._files[i];
            const mb = file.size / 1000 / 1000;
            const validFileType = this.isValidFileType(file.type);

            if (!validFileType) {
                if (this.validationError) {
                    this.validationError(FileDropValidationError.InvalidFileType);
                }
                
                return false;
            }
            
            if (mb > this._maxFileSize) {
                if (this.validationError) {
                    this.validationError(FileDropValidationError.TooLargeFile);
                }
                
                return false;
            }
        }
        
        return true;
    };
    
    FileDrop.prototype.isValidFileType = function (type) {
        if (this._allowedFileTypes.length === 0) {
            return true;
        }
        
        var allowed = false;
        for (var i = 0; i < this._allowedFileTypes.length; i++) {
            var temp = this._allowedFileTypes[i];
            if (temp.toLowerCase() === type.toLowerCase()) {
                allowed = true;
                break;
            }
        }
        
        return allowed;
    };
    
    FileDrop.prototype.setupListeners = function () {
        const _this = this;
        
        this.element.addEventListener('dragover', function (event) {
            event.preventDefault();
            if (_this.over) {
                _this.over(event);
            }
        }, false);
        
        this.element.addEventListener('drop', function (event) {
            event.preventDefault();
            _this._files = event.dataTransfer.files;
            _this.handleDrop();
        }, false);
        
        this.element.addEventListener('dragleave', function (event) {
            event.preventDefault();
            if (_this.leave) {
                _this.leave(event);
            }
        }, false);
        
        this.element.addEventListener('click', function (event) {
            event.preventDefault();

            _this._fileInput = document.createElement('input');
            _this._fileInput.type = 'file';
            _this._fileInput.setAttribute('multiple', 'multiple');
            
            if (_this._allowedFileTypes.length > 0) {
                _this._fileInput.accept = _this._allowedFileTypes.join(', ');
            }
            
            _this._fileInput.addEventListener('change', function (x) {
                if (this.files && this.files.length > 0) {
                    _this._files = this.files;
                    _this.handleDrop();
                }
            }, false);
            
            _this._fileInput.click();
        }, false);
        
        window.addEventListener('dragover', function (event) {
            event.preventDefault();
        }, false);
        
        window.addEventListener('drop', function (event) {
            event.preventDefault();
        }, false);
    };
    
    FileDrop.prototype.initOptions = function () {
        if (this.options) {
            if (this.options.hasOwnProperty('maxFileSize')) {
                if (!isNaN(this.options.maxFileSize)) {
                    this._maxFileSize = this.options.maxFileSize;
                }
            }
            
            if (this.options.hasOwnProperty('maxFileCount')) {
                if (!isNaN(this.options.maxFileCount)) {
                    this._maxFileCount = this.options.maxFileCount;
                }
            }
            
            if (this.options.hasOwnProperty('inputName')) {
                if (this.options.inputName) {
                    this._inputName = this.options.inputName;
                }
            }
            
            if (this.options.hasOwnProperty('allowedFileTypes')) {
                if (this.options.allowedFileTypes) {
                    this._allowedFileTypes = this.options.allowedFileTypes;
                }
            }
            
            if (this.options.hasOwnProperty('url')) {
                if (this.options.url) {
                    this._url = this.options.url;
                }
            }
        }
    };
    
    return FileDrop;
}());