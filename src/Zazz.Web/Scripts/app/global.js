﻿function showBtnBusy(btn) {
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

function loadAlbumsDropDownAsync(dropdownElem) {

    var def = $.Deferred();

    $.ajax({
        url: "/album/getalbums",
        error: function() {
            toastr.error("Failed to load the albums. Please try again later.");
            def.reject();
        },
        success: function (res) {
            


            def.resolve(res);
        }
    });

    return def.promise();
}

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