<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="MobileStartPage_v2.Default" EnableViewState="false" ViewStateMode="Disabled" %>

<!DOCTYPE HTML PUBLIC "-//WAPFORUM//DTD XHTML Mobile 1.2//EN" "http://www.wapforum.org/DTD/xhtml-mobile12.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" lang="ko" xml:lang="ko">
<head>
    <meta http-equiv="Content-Type" content="text/html" />
    <meta name="viewport" content="user-scalable=no, initial-scale=1.0, maximum-scale=1.0, minimum-scale=1.0, width=device-width" />
    <title>Mobile Start Page</title>
    <link href="/css/MobileStartPage.css" rel="stylesheet" />
    <style>
        ul {
        display:none;
        }
    </style>
    <script src="/js/jquery-1.6.4.js"></script>
    <script src="/js/common.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            $("#txtSearchWord").keydown(function (key) {
                if (key.keyCode == 13) {
                    var search_word = $("#txtSearchWord").val();
                    $("#txtSearchWord").val('');
                    SearchGoogle(search_word);
                }
            });

            $("#hdfCatId").val($("ul:first").attr("id"));
        });
        function ExpandCategory(id)
        {
            $("#hdfCatId").val(id);
            $("#" + id).show(200, function () {
                var offsetTop = ($("#" + id).offset().top * 1) - 30;
                $('html, body').animate({
                    scrollTop: offsetTop
                }, 200);
            });
        }
        function ToggleDisplayProp(id) {
            var cat_id_prev = $("#hdfCatId").val();

            if ($("#" + cat_id_prev).length == 0) {
                ExpandCategory(id);
            }
            else {
                if (id == cat_id_prev) {
                    $("#" + cat_id_prev).hide();
                    $("#hdfCatId").val('');
                }
                else {
                    $("#" + cat_id_prev).hide();
                    ExpandCategory(id);
                }
            }
        }
        function IsMobile() {
            var filter = "win16|win32|win64|mac";

            if (navigator.platform) {
                if (filter.indexOf(navigator.platform.toLowerCase()) >= 0) {
                    //데스크탑
                    return false;
                }
            }

            return true;
        }
        function MovePage(mobileUrl, desktopUrl) {
            if (IsMobile() == true) {
                //window.open(mobileUrl);//크롬에 홈버튼 보이면서 그냥 페이지 이동으로 변경
                document.location.href = mobileUrl;
            }
            else {
                if (desktopUrl == undefined || desktopUrl == null || desktopUrl == '') {
                    desktopUrl = mobileUrl;
                }
                document.location.href = desktopUrl;
            }
        }
        function NewTab() {
            window.open('about:blank');
        }
        function ShowEditSiteArea() {
            $("#divEditSite").css('display', 'block');
        }
        function HideEditSiteArea() {
            $("#divEditSite").css('display', 'none');
        }
        function AddSiteInfo(category_id) {
            $("#txtSiteIdx").val('');
            $("#txtSiteName").val('');
            $("#txtSiteUrl").val('');
            $("#txtSiteUrlMobile").val('');
            $("#txtSiteSort").val('');
            $("#ddlUseYN").val('Y');
            $("#ddlUseYN").attr('disabled', 'disabled');
        }
        function GetSiteInfo(site_idx)
        { 
            ShowEditSiteArea();
            $("#ddlUseYN").removeAttr('disabled');

            var url = "default.aspx";
            var postdata = GetPostData()
                            + "&SITE_IDX=" + site_idx
                            + "&AJAX_MODE=GET_SITE_INFO";

            CallAjax(url, postdata, {
                Run: function (data) {
                    if (data.site_idx != undefined && data.site_idx != null && site_idx != '') {
                        $("#ddlCategory").val(data.category_idx);
                        $("#txtSiteIdx").val(data.site_idx);
                        $("#txtSiteName").val(data.site_name);
                        $("#txtSiteUrl").val(data.site_url);
                        $("#txtSiteUrlMobile").val(data.site_url_mobile);
                        $("#txtSiteSort").val(data.site_sort);
                    }
                }
            }
                                    , Ajax_Fail);
        }
        function SaveSiteInfo() {
            var url = "default.aspx";
            var postdata = GetPostData()
                            + "&AJAX_MODE=SAVE_SITE_INFO";

            CallAjax(url, postdata, {
                Run: function (data) {
                    if (data == '') {
                        window.location.reload(true);
                    }
                    else {
                        Ajax_Fail.Run(data);
                    }
                }
            }
                                    , Ajax_Fail);
        }
        function SearchGoogle(search_word) {
            var url = "http://www.google.co.kr/cse";
            var param = "?q=" + encodeURIComponent(search_word);

            window.open(url + param);
        }
        var AddSite_Success = {
            Run: function (data) {
            }
        }
    </script>
</head>
<body>
    <div id="divMain">
        <div id="divNewTab">
            <span onclick="NewTab()">새탭 생성</span>
        </div>
        <div id="divSearch">
            <input id="txtSearchWord" type="text" style="width:90px" />
        </div>
        <!--북마크섹션 시작-->
        <input id="hdfCatId" type="hidden" />
        <%=GetMenuList() %>
        <!--북마크섹션 끝-->
    </div>
    <div id="divCategory">
        <span class="title">이름</span><input id="txtCategoryName" type="text" class="textbox" />
        <span class="title">순서</span><input id="txtCategorySort" type="text" class="textbox" />
    </div>
    <div id="divEditSite">
        <table style="width:100%;">
            <tr>
                <td style="width:140px">
                    <span class="title">카테고리</span>
                </td>
                <td>
                    <%=GetDdlCategory() %>
                </td>
            </tr>
            <tr>
                <td>
                    <span class="title">ID</span>
                </td>
                <td>
                    <input id="txtSiteIdx" type="text" class="textbox disabled" readonly="readonly" />
                </td>
            </tr>
            <tr>
                <td>
                    <span class="title">이름</span>
                </td>
                <td>
                    <input id="txtSiteName" type="text" class="textbox" />
                </td>
            </tr>
            <tr>
                <td>
                    <span class="title">URL</span>
                </td>
                <td>
                    <input id="txtSiteUrl" type="text" class="textbox" />
                </td>
            </tr>
            <tr>
                <td>
                    <span class="title">URL MOBILE</span>
                </td>
                <td>
                    <input id="txtSiteUrlMobile" type="text" class="textbox" />
                </td>
            </tr>
            <tr>
                <td>
                    <span class="title">순서</span>
                </td>
                <td>
                    <input id="txtSiteSort" type="text" class="textbox" />
                </td>
            </tr>
            <tr>
                <td>
                    <span class="title">사용</span>
                </td>
                <td>
                    <select id="ddlUseYN">
                        <option value="Y" selected="selected">사용</option>
                        <option value="N">사용안함</option>
                    </select>
                </td>
            </tr>
            <tr>
                <td></td>
                <td style="text-align:right">
                    <span style="cursor:pointer;" onclick="AddSiteInfo()">[신규]</span>
                    <span style="cursor:pointer;" onclick="SaveSiteInfo()">[저장]</span>
                    <span style="cursor:pointer" onclick="HideEditSiteArea()">[닫기]</span>
                </td>
            </tr>
        </table>
    </div>
</body>
</html>
