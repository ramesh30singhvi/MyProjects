
var imageCropper = {};


imageCropper.setImageUrl = function (base64Url) {
    if (imageCropper.cropper) {
        imageCropper.cropper.destroy();
        imageCropper.cropper = null;
    }

    document.getElementById('image').src = base64Url;

    imageCropper.cropper = new Cropper(image, {
        //aspectRatio: imageCropper.cropperConfiguration.width / imageCropper.cropperConfiguration.height,
        //viewMode: 3,
        autoCropArea:1,
        minContainerWidth: 400,
        minContainerHeight: 400,
        ready: imageCropper.readCropperImage,
        cropend: imageCropper.readCropperImage
    });
}

imageCropper.readCropperImage = function (event) {    
    imageCropper.cropper.getCroppedCanvas().toBlob((blob) => {
        var reader = new FileReader();
        if (blob instanceof Blob) {
            reader.readAsDataURL(blob);
            reader.onloadend = function () {
                var base64data = reader.result;
                imageCropper.base64data = base64data;
            }
        } 
    });
}

imageCropper.triggerFileSelect = function (id) {
    document.getElementById(id).click();
}

imageCropper.getCroppedImage = function () {
    return imageCropper.base64data;
}

imageCropper.initCropper = function (cropperConfiguration) {
    imageCropper.cropperConfiguration = cropperConfiguration;
}



