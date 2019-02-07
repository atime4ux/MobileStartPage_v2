function GetPostData() {
    var postData = "";

    $(':input')
        .not("#__EVENTTARGET, #__EVENTARGUMENT, #__LASTFOCUS, #__VIEWSTATE")
        .each(function (index) {
            if ($(this).val() != null) {
                if ($(this).val().length > 0) {
                    if (postData.length > 0) {
                        postData += '&';
                    }
                    postData += $(this).attr('id') + '=' + encodeURIComponent($(this).val());
                }
            }
        });

    return postData;
}

function CallAjax(target_url, postData, success_obj, fail_obj) {
    $.ajax({
        type: "POST",
        cache: false,
        async: true,
        url: target_url,
        data: postData,
        success: function (data) {
            if (success_obj != null && success_obj != undefined) {
                success_obj.Run(data);
            }
            else {
                return data;
            }
        },
        error: function (data) {
            if (fail_obj != null && fail_obj != undefined) {
                fail_obj.Run(data);
            }
            else {
                return data;
            }
        }
    });
}

var Ajax_Fail = {
    Run: function (result) {
        if (result == undefined || result == null || result == '') {
            alert('실패했습니다');
        }
        else {
            alert(result);
        }
    }
};