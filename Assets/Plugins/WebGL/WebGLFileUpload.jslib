mergeInto(LibraryManager.library, {
  ShowFileUpload: function (gameObjectName, methodName) {
    var input = document.createElement('input');
    input.type = 'file';
    input.accept = '.json';

    input.onchange = function (event) {
      var reader = new FileReader();
      reader.onload = function () {
        var content = reader.result;
        var contentBase64 = btoa(unescape(encodeURIComponent(content)));

        SendMessage(Pointer_stringify(gameObjectName), Pointer_stringify(methodName), contentBase64);
      };
      reader.readAsText(input.files[0]);
    };

    input.click();
  },

  DownloadFileFromUnity: function (base64Ptr, filenamePtr) {
    var base64 = UTF8ToString(base64Ptr);
    var filename = UTF8ToString(filenamePtr);

    var element = document.createElement('a');
    element.setAttribute('href', 'data:application/json;base64,' + base64);
    element.setAttribute('download', filename);
    element.style.display = 'none';
    document.body.appendChild(element);
    element.click();
    document.body.removeChild(element);
  }
});

