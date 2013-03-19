﻿var LOADING_INDICATOR = '<i class="icon-spin icon-refresh"></i>';

function showBtnBusy(btn) {
    var originalText = $(btn).html();

    var textWithSpinner = '<i style="margin-right:5px;" class="icon-refresh icon-spin"></i>' + originalText;

    $(btn).attr('disabled', 'disabled');
    $(btn).addClass('disabled');
    $(btn).html(textWithSpinner);

    return originalText;
}

function hideBtnBusy(btn, originalText) {
    $(btn).removeAttr('disabled', 'disabled');
    $(btn).removeClass('disabled');
    $(btn).html(originalText);
}

/////////////// SELECT IMAGE FROM GALLERY ////////////////

function loadAlbumsDropDownAsync(dropdownElem) {

    var def = $.Deferred();

    $.ajax({
        url: "/album/getalbums",
        error: function() {
            toastr.error("Failed to load the albums. Please try again later.");
            def.reject();
        },
        success: function (res) {

            var options = "";

            _.forEach(res, function(obj) {
                options += '<option value="' + obj.id + '">' + obj.name + '</option>';
            });

            $(dropdownElem).html(options);
            def.resolve(res);
        }
    });

    return def.promise();
}

function loadPGPhotos() {
    var albumId = $('#pg-albumSelect').val();
    if (albumId) {

        var url = "/album/getphotos";
        var photoContainer = $('#pg-photos');

        photoContainer.html(LOADING_INDICATOR);

        $.ajax({
            url: url,
            data: {
                albumId: albumId
            },
            error: function() {
                toastr.error("Failed to load photos. Please try again later");
            },
            success: function(res) {
                photoContainer.html(res);
            }
        });
    }
}

$('#pg-modal').on('show', function () {
    var loadAlbumTask = loadAlbumsDropDownAsync(document.getElementById("pg-albumSelect"));

    loadAlbumTask.done(function(res) {
        loadPGPhotos();
    });
});

$(document).on('click', '*[data-ajax-pagination] a', function(e) {
    e.preventDefault();

    var url = $(this).attr('href');
    var panelToUpdate = $($(this).closest('*[data-ajax-pagination]').attr('data-update'));
    panelToUpdate.html(LOADING_INDICATOR);

    $.ajax({
        url: url,
        error: function() {
            toastr.error("An error occured");
        },
        success: function(res) {
            panelToUpdate.html(res);
        }
    });

});

$(document).on('click', 'button[data-selectPhoto]', function (e) {

    e.preventDefault();

    var id = $(this).data('id');
    var imgUrl = $(this).data('url');
    
    $('#photoId').val(id);
    $('#selectedImg-thumbnail').attr('src', imgUrl);

    $('#pg-modal').modal('hide');
});



$(document).on('change', '#pg-albumSelect', function () {
    loadPGPhotos();
});

///////////////////////UPLOAD PHOTO MODAL////////////////////////////////



/////////////////////////////////////////////////////////////////////////

$(function () {
    
    $('.datepicker').datetimepicker();

    $('*[title]').tooltip();

    $('#party-web-link').click(function() {
        var url = "/follow/GetFollowRequests";

        $.ajax({
            url: url,
            cache: false,
            error: function() {
                toastr.error("An error occured, Please try again later.");
            },
            success: function(data) {
                var container = $('#party-web-requests-body');
                container.fadeOut('slow', function() {
                    container.html(data);
                    container.fadeIn('slow');
                });
            }
        });

    });

    
})