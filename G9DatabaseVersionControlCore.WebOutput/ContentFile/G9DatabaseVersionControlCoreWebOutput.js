

window.onload = function() {
    if (window.jQuery) {
        // jQuery is loaded  - Ignore
    } else {
        // jQuery is not loaded
        alert("Jquery not available!");
    }
};

$(document).ready(function() {

    // فیلد برای نگهداری نوع نصب درخواستی که در مرحله دوم توسط کاربر مقدار دهی می شود
    var TypeOfInstall = -1;

    // متغیر برای نگهداری تمام اطلاعات پروژه های در دسترس
    var ProjectsMapData = [
        {
            ProjectName: "",
            ProjectVersion: "",
            DatabaseName: "",
            DatabaseVersion: "",
            ExistBaseDatabase: false,
            CustomTasksItems: [
                {
                    Nickname: "",
                    Description: ""
                }
            ],
            EnableSetCustomDatabaseName: false,
            EnableSetCustomDatabaseRestoreFilePath: false
        }
    ];

    // متغیر برای نگهداری نام مستعار تسک دلخواه 
    var CustomTaskNickName = null;

    // پروژه انتخاب شده را مشخص می کند
    var ChooseProjectIndex;
    var ChooseProjectName;

    // Enum to specifies task request
    var G9ETaskRequest = {
        // Request for enter connection string
        EnterConnectionString: 0,
        // Request for check exist database
        CheckExistDatabase: 1,
        // Request for install software
        InstallSoftwareAndUpdate: 2,
        // Request for update
        UpdateSoftware: 3,
        // Request for custom task
        CustomTask: 4,
        // Request for get last status
        CheckLastStatus: 5
    };

    // تمامی صفح ها در ابتدا باید از دسترس خارج باشند.
    $(".Pages").fadeOut();

    let DataSourceTXTB;

    setTimeout(function() {
            $(".MainDiv_CenterDiv").animate({ "height": "400px" }, { duration: 1369, queue: true });
            $(".MainDiv_CenterDiv_SetConnectionString").fadeIn({ duration: 969, queue: true });
        },
        999);

    // ##################################################### هندلر قسمت عکس های بک گراند سایت #####################################################
    var InstallBgImages = $(".Install_BG_Image");
    var InstallBGCountener1 = 0;

    $(InstallBgImages).fadeOut();
    $(InstallBgImages[InstallBGCountener1]).fadeIn();

    HandleInstallBackground();

    // حلقه زمانی برای تغییر عکس بکگراند در زمان مشخص
    setInterval(function() {
            HandleInstallBackground();
        },
        12000);

    // تابع برای هندل کردن عکس های بکگراند زمان نصب
    function HandleInstallBackground() {
        var TempTop, TempLeft;

        var Current = InstallBGCountener1;

        switch (Math.floor((Math.random() * 2)) + 1) {
        case 1:
            TempTop = "-19%";
            TempLeft = "-19%";
            $(InstallBgImages[Current]).css({ "left": "0%", "top": "0%" });
            break;
        case 2:
            TempTop = "-1%";
            TempLeft = "-1%";
            $(InstallBgImages[Current]).css({ "left": "-19%", "top": "-19%" });
            break;
        case 3:
            TempTop = "-19%";
            TempLeft = "-1%";
            $(InstallBgImages[Current]).css({ "left": "-19%", "top": "-1%" });
            break;
        case 4:
            TempTop = "-1%";
            TempLeft = "-19%";
            $(InstallBgImages[Current]).css({ "left": "-1%", "top": "-19%" });
            break;
        default:
            return;
        }

        InstallBGCountener1 = InstallBGCountener1 + 1;
        if (InstallBGCountener1 >= $(InstallBgImages).length) {
            InstallBGCountener1 = 0;
        }

        $(InstallBgImages[Current]).fadeIn({ duration: 3000, queue: false });


        $(InstallBgImages[Current]).animate({ "left": TempTop, "top": TempLeft },
            { duration: 13000, queue: false, easing: "linear" });

        setTimeout(function() {
                $(InstallBgImages[Current]).fadeOut({ duration: 3000, queue: false });
            },
            11000);
    }
    // ##################################################### هندلر قسمت عکس های بک گراند سایت #####################################################


    // ##################################################### هندلر قسمت نمایش ارور #####################################################

    // آماده سازی و مقدار دهی اولیه آیتم های نمایشی پیام
    var FuncIfAccept, FuncIfDenied;
    $(".MainDiv_CenterDiv_MessageDiv, .MainDiv_CenterDiv_CustomTaskDiv").fadeOut();
    $(".MainDiv_CenterDiv_MessageDiv_MainDiv").hide();
    $(".MainDiv_CenterDiv_MessageDiv_MainDiv").children().fadeOut();
    $(".MainDiv_CenterDiv_MessageDiv_MainDiv").css({ "height": "0px" });

    // تابع برای نمایش پیام ها
    // پارامتر اول پیام جهت نمایش، پارامتر دوم اگر نیاز به دکمه تایید و انصراف داشت، گارامتر سوم اگر پیام یک ارور بود
    // پارامتر چهارم فانکشنی که بعد از زدن دکمه تایید باید اجرا شود و پارامتر آخر تابعی که باید با زدن دکمه انصراف اجرا شود
    // دو پارامتر آخر فقط در حالتی استفاده خواهند شد که پارامتر دوم "ترو" ارسال شود
    function ShowMessage(Message, HaveAcceptAndDenied, IsErrorMessage, FunctionIfAcceptOrOK, FunctionIfDenied) {

        $("#MainDiv_CenterDiv_MessageDiv_MainDiv_Lable").text(Message);

        if (IsErrorMessage) {
            $(".MainDiv_CenterDiv_MessageDiv_MainDiv").css({ "background-color": "#dc143c" });
        } else {
            $(".MainDiv_CenterDiv_MessageDiv_MainDiv").css({ "background-color": "#006400" });
        }

        if (HaveAcceptAndDenied) {
            $("#MainDiv_CenterDiv_Row_Close").addClass("hidden");
            $("#MainDiv_CenterDiv_Row_AcceptAndDenied").removeClass("hidden").fadeOut();

            FuncIfAccept = FuncIfDenied = null;

            if (FunctionIfAcceptOrOK != null) {
                FuncIfAccept = FunctionIfAcceptOrOK;
            }

            if (FunctionIfDenied != null) {
                FuncIfDenied = FunctionIfDenied;
            }
        } else {
            $("#MainDiv_CenterDiv_Row_Close").removeClass("hidden").fadeOut();
            $("#MainDiv_CenterDiv_Row_AcceptAndDenied").addClass("hidden");

            if (FunctionIfAcceptOrOK != null) {
                FuncIfAccept = FunctionIfAcceptOrOK;
            }
        }

        $(".MainDiv_CenterDiv_MessageDiv").fadeIn(369,
            function() {
                $(".MainDiv_CenterDiv_MessageDiv_MainDiv").show();
                $(".MainDiv_CenterDiv_MessageDiv_MainDiv").animate({ "height": "99px" },
                    369,
                    function() {
                        $(".MainDiv_CenterDiv_MessageDiv_MainDiv").children().fadeIn(99);
                    });
            });
    }

    // تابع برای بستن پیام
    function CloseMessage() {
        $(".MainDiv_CenterDiv_MessageDiv_MainDiv").children().fadeOut(99,
            function() {
                $(".MainDiv_CenterDiv_MessageDiv_MainDiv").animate({ "height": "0px" },
                    369,
                    function() {
                        $(".MainDiv_CenterDiv_MessageDiv").fadeOut(369);
                    });
            });
    }

    // تابع هندلر برای رویداد کلید بستن
    $("#MainDiv_CenterDiv_MessageDiv_MainDiv_Close").click(function() {
        CloseMessage();

        if (FuncIfAccept != null) {
            FuncIfAccept();
        }
    });

    // تابع هندلر برای رویداد انصراف
    $("#MainDiv_CenterDiv_MessageDiv_MainDiv_Accept").click(function() {

        if (FuncIfAccept != null) {
            FuncIfAccept();
        }

        CloseMessage();
    });

    // تابع هندلر برای رویداد انصراف
    $("#MainDiv_CenterDiv_MessageDiv_MainDiv_Denied").click(function() {

        if (FuncIfDenied != null) {
            FuncIfDenied();
        }

        CloseMessage();
    });

    // ##################################################### هندلر قسمت نمایش ارور #####################################################


    // ##################################################### هندلر قسمت اول چک کردن کانکشن استرینگ #####################################################


    $("#ConnectionSettingSelect").on("change",
        function() {
            const selector = $(this).find(":selected");
            const dataSource = $(selector).attr("datasource");
            const userId = $(selector).attr("userid");
            const password = $(selector).attr("password");
            if (dataSource === "G9Custom") {
                $("#DB_DataSource, #DB_UserId, #DB_Password").val("");
                $("#DB_DataSource, #DB_UserId, #DB_Password").prop("disabled", false);
            } else {
                $("#DB_DataSource").val(dataSource);
                $("#DB_UserId").val(userId);
                $("#DB_Password").val(password);
                $("#DB_DataSource, #DB_UserId, #DB_Password").prop("disabled", "disabled");
            }
        });

    $("#Btn_CheckConnectionString").click(function() {

        if ($("#DB_DataSource").val() != null &&
            $("#DB_DataSource").val().trim().length > 0 &&
            $("#DB_UserId").val() != null &&
            $("#DB_UserId").val().trim().length > 0 &&
            $("#DB_Password").val() != null &&
            $("#DB_Password").val().trim().length > 0
        ) {

            $(".MainDiv_CenterDiv_SetConnectionString").fadeOut(369,
                function() {
                    StartLoading();
                });

            // CheckConnectionString(string oDB_DataSource, string oDB_UserId, string oDB_Password)

            DataSourceTXTB = $("#DB_DataSource").val();
            if (DataSourceTXTB.indexOf("\\") >= 0) {
                DataSourceTXTB = DataSourceTXTB.replace("\\", "\\\\");
            }

            SendAndReceiveDataAjax(ReadySendPacket(G9ETaskRequest.EnterConnectionString,
                    `{DataSource: '${DataSourceTXTB}',  UserId: '${$("#DB_UserId").val()}', Password: '${$(
                        "#DB_Password").val()}'}`),
                // Success
                function(result) {

                    if (result.Success) {
                        ProjectsMapData = JSON.parse(result.Data);
                        var existProjectWithBaseDatabase = false;
                        var existProjectWithCustomTask = false;
                        $.each(ProjectsMapData,
                            function(index) {
                                if (ProjectsMapData[index].ExistBaseDatabase)
                                    existProjectWithBaseDatabase = true;
                                if (ProjectsMapData[index].CustomTasksItems)
                                    existProjectWithCustomTask = true;
                            });
                        if (!existProjectWithBaseDatabase)
                            $(".MainDiv_CenterDiv_Cul[itemtype='1']").hide();
                        if (!existProjectWithCustomTask)
                            $(".MainDiv_CenterDiv_Cul[itemtype='2']").hide();
                        setTimeout(function() {
                                $(".Btn_CheckConnectionString").css("height", "");
                                StopLoading(function() {
                                    $(".MainDiv_CenterDiv_SetInstallData").fadeIn(369);
                                });
                            },
                            1696);
                    } else {
                        setTimeout(function() {
                                StopLoading(function() {
                                    $(".MainDiv_CenterDiv_SetConnectionString").fadeIn(369);
                                });
                            },
                            1696);
                    }
                },
                // Error
                function(result) {
                    ShowMessage("The server has encountered a problem!",
                        false,
                        true);
                    setTimeout(function() {
                            StopLoading(function() {
                                $(".MainDiv_CenterDiv_SetConnectionString").fadeIn(369);
                            });
                        },
                        1696);
                });

        } else {
            ShowMessage("Please enter the fields associated with the database connection string!",
                false,
                true);
        }

    });

    // ##################################################### هندلر قسمت اول چک کردن کانکشن استرینگ #####################################################


    // ##################################################### هندلر قسمت دوم انتخاب نوع نصب #####################################################

    var TypeOfInstall = null;
    $(".MainDiv_CenterDiv_Cul").click(function() {

        TypeOfInstall = parseInt($(this).attr("itemtype"));

        $("#ChooseProjectTbody").empty();
        $.each(ProjectsMapData,
            function(index) {
                if (TypeOfInstall === 1 && ProjectsMapData[index].ExistBaseDatabase) {
                    $("#ChooseProjectTbody").append(
                        `<tr projectIndex="${index}" projectname="${ProjectsMapData[index].ProjectName}">
                            <td>${ProjectsMapData[index].ProjectName}</td>
                            <td>${ProjectsMapData[index].ProjectVersion}</td>
                            <td>${ProjectsMapData[index].DatabaseName}</td>
                            <td>${ProjectsMapData[index].DatabaseVersion}</td>
                        </tr>`);
                } else if (TypeOfInstall === 2 && ProjectsMapData[index].CustomTasksItems) {
                    $("#ChooseProjectTbody").append(
                        `<tr projectIndex="${index}" projectname="${ProjectsMapData[index].ProjectName}">
                            <td>${ProjectsMapData[index].ProjectName}</td>
                            <td>${ProjectsMapData[index].ProjectVersion}</td>
                            <td>${ProjectsMapData[index].DatabaseName}</td>
                            <td>${ProjectsMapData[index].DatabaseVersion}</td>
                        </tr>`);
                } else if (TypeOfInstall === 3) {
                    $("#ChooseProjectTbody").append(
                        `<tr projectIndex="${index}" projectname="${ProjectsMapData[index].ProjectName}">
                            <td>${ProjectsMapData[index].ProjectName}</td>
                            <td>${ProjectsMapData[index].ProjectVersion}</td>
                            <td>${ProjectsMapData[index].DatabaseName}</td>
                            <td>${ProjectsMapData[index].DatabaseVersion}</td>
                        </tr>`);
                }
            });

        $(".MainDiv_CenterDiv_SetInstallData").fadeOut(369,
            function() {
                $(".MainDiv_CenterDiv_ChooseProject").fadeIn(369);
            });
    });

    // ##################################################### هندلر قسمت دوم انتخاب نوع نصب #####################################################

    // ##################################################### هندلر قسمت سوم وارد انتخاب پروژه  #####################################################

    $(document).on("click",
        "tr[projectname]",
        function() {
            debugger;
            ChooseProjectName = $(this).attr("projectname");
            ChooseProjectIndex = parseInt($(this).attr("projectIndex"));
            if (TypeOfInstall === 2) {
                var optionItems = "";
                $.each(ProjectsMapData[ChooseProjectIndex].CustomTasksItems,
                    function(index, item) {
                        optionItems += `<option description="${item.Description}">${item.Nickname}</option>`;
                    });
                $("#MainDiv_CenterDiv_CustomTaskDiv_Select_CustomTaskNickName").empty();
                $("#MainDiv_CenterDiv_CustomTaskDiv_Select_CustomTaskNickName").append(optionItems);
                $("#MainDiv_CenterDiv_CustomTaskDiv_Description_CustomTaskNickName")
                    .text(ProjectsMapData[ChooseProjectIndex].CustomTasksItems[0].Description);
                $("#MainDiv_CenterDiv_CustomTaskDiv_MainDiv_Accept").attr("projectname", ChooseProjectName);
                $("#MainDiv_CenterDiv_CustomTaskDiv_MainDiv_Accept").attr("projectIndex", ChooseProjectIndex);
                $(".MainDiv_CenterDiv_CustomTaskDiv").fadeIn(369);
            } else {
                $("#Input_NewDBName").val(ProjectsMapData[ChooseProjectIndex].DatabaseName);
                if (ProjectsMapData[ChooseProjectIndex].EnableSetCustomDatabaseName) {
                    $("#Input_NewDBName").prop("disabled", false);
                } else {
                    $("#Input_NewDBName").prop("disabled", true);
                }
                $(".MainDiv_CenterDiv_ChooseProject").fadeOut(369,
                    function() {
                        $(".MainDiv_CenterDiv_SetDataForInstall").fadeIn(369);
                    });
            }
        });

    $("#MainDiv_CenterDiv_CustomTaskDiv_Select_CustomTaskNickName").change(function() {
        $("#MainDiv_CenterDiv_CustomTaskDiv_Description_CustomTaskNickName")
            .text($(this).find(":selected").attr("description"));
    });

    $("#MainDiv_CenterDiv_CustomTaskDiv_MainDiv_Denied").click(function() {
        $(".MainDiv_CenterDiv_CustomTaskDiv").fadeOut(199);
    });

    $("#MainDiv_CenterDiv_CustomTaskDiv_MainDiv_Accept").click(function() {
        CustomTaskNickName = $("#MainDiv_CenterDiv_CustomTaskDiv_Select_CustomTaskNickName").val();
        $(".MainDiv_CenterDiv_CustomTaskDiv").fadeOut(39);
        ChooseProjectName = $(this).attr("projectname");
        ChooseProjectIndex = parseInt($(this).attr("projectIndex"));
        $("#Input_NewDBName").val(ProjectsMapData[ChooseProjectIndex].DatabaseName);
        if (ProjectsMapData[ChooseProjectIndex].EnableSetCustomDatabaseName) {
            $("#Input_NewDBName").prop("disabled", false);
        } else {
            $("#Input_NewDBName").prop("disabled", true);
        }
        $(".MainDiv_CenterDiv_ChooseProject").fadeOut(369,
            function() {
                $(".MainDiv_CenterDiv_SetDataForInstall").fadeIn(369);
            });
    });

    // ##################################################### هندلر قسمت سوم وارد انتخاب پروژه  #####################################################


    // ##################################################### هندلر قسمت سوم وارد کردن نام دیتابیس  #####################################################

    // تابع هندلر برای هندل کردن رویداد دکمه بازگشت به قبل
    $("#Btn_BackStep2").click(function() {
        //$(".MainDiv_CenterDiv_SetDataForInstall").fadeOut(369);
        $(".MainDiv_CenterDiv_ChooseProject").fadeOut(369,
            function() {
                $(".MainDiv_CenterDiv_SetInstallData").fadeIn(369);
            });
    });

    $("#Btn_BackStep2_1").click(function() {
        //$(".MainDiv_CenterDiv_SetDataForInstall").fadeOut(369);
        $(".MainDiv_CenterDiv_SetDataForInstall").fadeOut(369,
            function() {
                $(".MainDiv_CenterDiv_ChooseProject").fadeIn(369);
            });
    });

    // هندلر برای رویداد دکمه ادامه نصب
    $("#Btn_NextStep4").click(function() {
        HandleSelectDatabaseName();
    });

    function HandleSelectDatabaseName() {
        if ($("#Input_NewDBName").val() == null ||
            $("#Input_NewDBName").val().trim().length == 0) {

            ShowMessage("لطفا فیلد های خالی را پر کنید",
                false,
                true);
            return;
        }

        $(".MainDiv_CenterDiv_SetDataForInstall").fadeOut(369,
            function() {
                StartLoading();
            });

        // Custom Task
        if (TypeOfInstall == 2) {
            CheckDbExist($("#Input_NewDBName").val(),
                // تابع برای هندل اگر وجود داشت
                function() {
                    StartInstallAndUpdate();
                },
                // تابع برای هندل در صورت عدم وجود دیتابیس
                function() {
                    ShowMessage("There is no database with this name.",
                        false,
                        true,
                        StopLoading(function() {
                            setTimeout(function() {
                                    StopLoading(function() {
                                        $(".MainDiv_CenterDiv_SetDataForInstall").fadeIn(369);
                                    });
                                },
                                639);
                        }));
                }
            );
        }
        // Update
        else if (TypeOfInstall == 3) {

            CheckDbExist($("#Input_NewDBName").val(),
                // تابع برای هندل اگر وجود داشت
                function() {
                    StartInstallAndUpdate();
                },
                // تابع برای هندل در صورت عدم وجود دیتابیس
                function() {
                    ShowMessage("There is no database with this name.",
                        false,
                        true,
                        StopLoading(function() {
                            setTimeout(function() {
                                    StopLoading(function() {
                                        $(".MainDiv_CenterDiv_SetDataForInstall").fadeIn(369);
                                    });
                                },
                                639);
                        }));
                }
            );

        }
        // Install And Update
        else if (TypeOfInstall == 1) {

            CheckDbExist($("#Input_NewDBName").val(),
                // اگر وجود داشت
                function() {
                    ShowMessage("نام بانک اطلاعاتی جدید جهت نصب داده ها وجود دارد، آیا می خواهید آن را حذف کنید!؟",
                        true,
                        false,
                        // اگر اکسپت کرد
                        function() {

                            SendAndReceiveDataAjax("RemoveDB",
                                `{'DBName': '${$("#Input_NewDBName").val()}'}`,
                                // Success
                                function(resualt) {
                                    if (resualt.Success) {
                                        HandleSelectDatabaseName();
                                    } else {
                                        setTimeout(function() {
                                                StopLoading(function() {
                                                    $(".MainDiv_CenterDiv_SetDataForInstall").fadeIn(369);
                                                });
                                            },
                                            1696);
                                        return;
                                    }
                                },
                                // Error
                                function(resualt) {
                                    ShowMessage(resualt,
                                        false,
                                        true);
                                    setTimeout(function() {
                                            StopLoading(function() {
                                                $(".MainDiv_CenterDiv_SetDataForInstall").fadeIn(369);
                                            });
                                        },
                                        1696);
                                    return;
                                });

                        },
                        // اگر اکسپت نکرد
                        function() {
                            setTimeout(function() {
                                    StopLoading(function() {
                                        $(".MainDiv_CenterDiv_SetDataForInstall").fadeIn(369);
                                    });
                                },
                                1696);
                            return;
                        });
                    return;
                    // اگر وجود تداشت
                },
                function() {
                    if (ProjectsMapData[ChooseProjectIndex].EnableSetCustomDatabaseRestoreFilePath) {
                        $("#Input_SetDbURL").prop("disabled", false);
                    } else {
                        $("#Input_SetDbURL").prop("disabled", true);
                    }
                    setTimeout(function() {
                            StopLoading(function() {
                                $(".MainDiv_CenterDiv_SetSoftwareName").fadeIn(369);
                            });
                        },
                        1696);
                });

        }
    }

    function CheckDbExist(DbName, runIfExists, runIfNotExists, runIfError) {
        SendAndReceiveDataAjax(ReadySendPacket(G9ETaskRequest.CheckExistDatabase,
                `{DataSource: '${DataSourceTXTB}',  UserId: '${$("#DB_UserId").val()}', Password: '${$(
                    "#DB_Password").val()}', DatabaseName: '${DbName}'}`),
            // Success
            function(resualt) {
                if (resualt.Success) {
                    if (runIfExists != null) {
                        runIfExists();
                    }
                } else {
                    if (runIfNotExists != null) {
                        runIfNotExists();
                    }
                }
            },
            // Error
            function(resualt) {
                ShowMessage(resualt,
                    false,
                    true);
                if (runIfError != null) {
                    runIfError();
                }
            });
    }


    // ##################################################### هندلر قسمت سوم وارد کردن نام دیتابیس #####################################################


    // ##################################################### هندلر قسمت چهارم وارد کردن نام شرکت #####################################################

    // تابع هندلر برای هندل کردن رویداد دکمه بازگشت به قبل
    $("#Btn_BackStep3").click(function() {
        $(".MainDiv_CenterDiv_SetSoftwareName").fadeOut(369,
            function() {
                $(".MainDiv_CenterDiv_SetDataForInstall").fadeIn(369);
            });
    });

    // تابع هندلر برای شروع نصب برنامه
    $("#Btn_StartInstall").click(function() {
        StartInstallAndUpdate();
    });

    // تابع برای هندل کردن مسیر دیتابیس
    $("#Input_SetDbURL").click(function() {
        $("#Input_SetDbURL_Text").fadeOut(0);
        $("#Input_SetDbURL").fadeOut(369,
            function() {
                $("#Input_SetDbURL_Text").removeClass("hidden");
                $("#Input_SetDbURL_Text").fadeIn(369);
            });
    });
    String.prototype.replaceAll = function(searchStr, replaceStr) {
        const str = this;

        // escape regexp special characters in search string
        searchStr = searchStr.replace(/[-\/\\^$*+?.()|[\]{}]/g, "\\$&");

        return str.replace(new RegExp(searchStr, "gi"), replaceStr);
    };

    function StartInstallAndUpdate() {

        var DBURL = $("#Input_SetDbURL_Text").val();

        if (!ProjectsMapData[ChooseProjectIndex].EnableSetCustomDatabaseRestoreFilePath ||
            DBURL === null ||
            DBURL === "") {
            DBURL = null;
        } else {
            DBURL = DBURL.replaceAll("\\", "\\\\");
        }

        var databaseName = $("#Input_NewDBName").val();
        if (!ProjectsMapData[ChooseProjectIndex].EnableSetCustomDatabaseName)
            databaseName = ProjectsMapData[ChooseProjectIndex].DatabaseName;
        debugger;
        StartLoading(function() {
            SendAndReceiveDataAjax(ReadySendPacket(
                    TypeOfInstall == 3
                    ? G9ETaskRequest.UpdateSoftware
                    : TypeOfInstall == 1
                    ? G9ETaskRequest.InstallSoftwareAndUpdate
                    : G9ETaskRequest.CustomTask,
                    `{DataSource: '${DataSourceTXTB}',  UserId: '${$("#DB_UserId").val()}', Password: '${
                    $("#DB_Password").val()}',  DatabaseName: '${databaseName}',  ProjectName: '${
                    ProjectsMapData[ChooseProjectIndex].ProjectName}', CustomDatabaseRestorePath: ${DBURL}, CustomTaskNickname: '${CustomTaskNickName}'}`),
                function(resualt) {
                    if (resualt.Success) {
                        setTimeout(function() {
                                StopLoading(function() {
                                    $(".MainDiv_CenterDiv_StartInstall").fadeIn(99,
                                        function() {
                                            StartInstall();
                                        });
                                });
                            },
                            3969);
                    } else {
                        $(".MainDiv_CenterDiv_SetDataForInstall").fadeIn(369);
                        ShowMessage("نصب با مشکل مواجه شده است لطفا لاگ را چک کنید", false, true);
                        StopLoading();
                    }
                },
                function() {
                    $(".MainDiv_CenterDiv_SetDataForInstall").fadeIn(369);
                    ShowMessage("نصب با مشکل مواجه شده است لطفا لاگ را چک کنید", false, true);
                    StopLoading();
                });
        });
    }


    // ##################################################### هندلر قسمت چهارم وارد کردن نام شرکت #####################################################


    // ##################################################### هندلر قسمت پنجم نصب برنامه #####################################################

    $(
            ".MainDiv_CenterDiv_StartInstall_IMG, .MainDiv_CenterDiv_StartInstall_Proccess, .MainDiv_CenterDiv_StartInstall_Infromation")
        .fadeOut();
    $(".MainDiv_CenterDiv_ItemCount_ChildDiv").children().fadeOut(0);
    $(".MainDiv_CenterDiv_ItemCount_ChildDiv").fadeOut();

    // تابع اولیه برای نمایش شروع نصب
    $("#Lable_For_Timer").fadeOut(0);
    var H = 0, M = 0, S = 0, TimerInterval;

    function StartInstall() {

        $(".MainDiv_CenterDiv_LogoImg").fadeOut({ duration: 1369, queue: false });
        $(".MainDiv_CenterDiv_Header_TitleIMG").css({ "margin": "0 auto", "margin-top": "9.9px" });
        $(".MainDiv_CenterDiv_Header_TitleIMG").animate({ "top": "-=10px", "width": "-=150px" },
            { duration: 1369, queue: false });

        setTimeout(function() {
                $("#Lable_For_Timer").fadeIn(369);
            },
            1369);

        TimerInterval = setInterval(function() {
                S++;
                if (S > 59) {
                    S = 0;
                    M++;
                    if (M > 59) {
                        M = 0;
                        H++;
                    }
                }
                $("#Lable_For_Timer").text(
                    (H < 10 ? `0${H}` : H) +
                    ":" +
                    (M < 10 ? `0${M}` : M) +
                    ":" +
                    (S < 10 ? `0${S}` : S));
            },
            1000);


        $(".MainDiv_CenterDiv_StartInstall_IMG[iteminf='Shadow']").css({ "top": "+=20px" })
            .fadeIn({ duration: 1369, queue: false });
        $(".MainDiv_CenterDiv_StartInstall_IMG[iteminf='Shadow']").animate({ "top": "-=20px" },
            1369,
            function() {
                ShowInstallState(FlagInstallState,
                    function() {
                        CheckLastStatusOfInstall();
                    });
            });
    }

    // فلگ برای نگهداری مرحله نصب
    var FlagInstallState = 0;

    // فیلد ها برای نگهداری مراحل و درصد آن
    var CheckInstallData = 0;
    var RestoreEmptyDataBase = 0;
    var ConverFromOldDbToNewDb = 0;
    var UpdateDataBase = 0;

    // تعداد آیتم ها برای جا به جایی
    var ItemCountForConvert = 0;

    // فیلد برای نگهداری آخرین داده دریافت شده از سایت
    var LastItemReceive = null;

    // تابع برای گرفتن آخرین وضعیت نصب
    function CheckLastStatusOfInstall() {
        setTimeout(function() {
                SendAndReceiveDataAjax(ReadySendPacket(G9ETaskRequest.CheckLastStatus, ""),
                    // اگر تماس به درستی بر قرار شد
                    function(result) {

                        LastItemReceive = result;

                        if (result.Success) {

                            //console.log(result.toSource());

                            let RunAgain = true;

                            if (result.RowReceiveNumberCount > 0) {
                                ItemCountForConvert = result.RowReceiveNumberCount;
                            }

                            switch ((result.StepOfInstall - 2)) {
                            case 0:
                                CheckInstallData = result.PercentCurrectStep;
                                break;
                            case 1:
                                CheckInstallData = 100;
                                RestoreEmptyDataBase = result.PercentCurrectStep;
                                break;
                            case 2:
                                CheckInstallData = 100;
                                RestoreEmptyDataBase = 100;
                                ConverFromOldDbToNewDb = result.PercentCurrectStep;
                                break;
                            case 3:
                                CheckInstallData = 100;
                                RestoreEmptyDataBase = 100;
                                ConverFromOldDbToNewDb = 100;
                                UpdateDataBase = result.PercentCurrectStep;
                                break;
                            case 4:
                                // Finish
                                CheckInstallData = 100;
                                RestoreEmptyDataBase = 100;
                                ConverFromOldDbToNewDb = 100;
                                UpdateDataBase = 100;
                                RunAgain = false;
                                break;
                            default:
                                ShowMessage("UnHandle Step!", false, true);
                                return;
                            }

                            if (RunAgain) {
                                return CheckLastStatusOfInstall();
                            }
                        }
                    },
                    // اگر با مشکل رو به رو شد
                    function(result) {
                        ShowMessage(result,
                            false,
                            true);
                    });
            },
            369);
    }

    var FlagLock = false;

    // تابع برای هندل کردن مراحل نصب
    var CheckStatusAndShowIt = setInterval(function() {

            if (FlagLock) {
                return;
            }

            switch (FlagInstallState) {
            case 0:
                if (CheckInstallData >= 100) {
                    FlagLock = true;
                    SetInstallProcess(100,
                        963,
                        function() {
                            FlagInstallState++;
                            ShowInstallState(FlagInstallState,
                                function() {
                                    FlagLock = false;
                                });
                        });
                } else {
                    SetInstallProcess(CheckInstallData, 963);
                }
                break;
            case 1:

                if (ItemCountForConvert != -99) {
                    ShowItemCount(ItemCountForConvert);
                    ItemCountForConvert = -99;
                }

                if (RestoreEmptyDataBase >= 100) {
                    FlagLock = true;
                    SetInstallProcess(100,
                        963,
                        function() {
                            FlagInstallState++;
                            ShowInstallState(FlagInstallState,
                                function() {
                                    FlagLock = false;
                                });
                        });
                } else {
                    SetInstallProcess(RestoreEmptyDataBase, 963);
                }
                break;
            case 2:
                if (ConverFromOldDbToNewDb >= 100) {
                    FlagLock = true;
                    SetInstallProcess(100,
                        963,
                        function() {
                            FlagInstallState++;
                            ShowInstallState(FlagInstallState,
                                function() {
                                    FlagLock = false;
                                });
                        });
                } else {
                    SetInstallProcess(ConverFromOldDbToNewDb, 963);
                }
                break;
            case 3:
                // Finish
                if (UpdateDataBase >= 100) {
                    clearInterval(CheckStatusAndShowIt);
                    FlagLock = true;
                    SetInstallProcess(100,
                        963,
                        function() {
                            setTimeout(function() {
                                    ShowFinishedProgram();
                                },
                                969);
                        });
                } else {
                    SetInstallProcess(UpdateDataBase, 963);
                }
                break;
            default:
                ShowMessage("UnHandle Step!", false, true);
                return;
            }
        },
        999);

    // تابع برای نمایش قسمت های در حال نصب
    function ShowInstallState(State, FuncStart) {

        var InstallTitle, InstallInfo;

        var ItmeInfoStartBy = "";

        switch (State) {
        case 0:
            InstallTitle = "بررسی و آماده سازی";
            InstallInfo = "قبل از شروع نصب باید آیتم ها بررسی و آماده شود تا مشکلی در روند نصب وجود نداشته باشد.";
            ItmeInfoStartBy = "A";
            break;
        case 1:
            InstallTitle = "ساخت دیتابیس جدید";
            InstallInfo = "برای انجام عملیات نصب باید دیتابیس جدید ایجاد شده و داده های پیش فرض نصب شود.";
            ItmeInfoStartBy = "B";
            break;
        case 2:
            InstallTitle = "تبدیل دادها";
            InstallInfo = "تبدیل داده ها از بانک قدیم به بانک جدید انجام می شود";
            ItmeInfoStartBy = "C";
            break;
        case 3:
            InstallTitle = "بروزرسانی";
            InstallInfo = "بانک اطلاعاتی باید به آخرین نسخه بروزرسانی شود";
            ItmeInfoStartBy = "D";
            break;
        default:
            return;
        }


        $(".MainDiv_CenterDiv_StartInstall_Proccess, .MainDiv_CenterDiv_StartInstall_Infromation")
            .fadeOut({ duration: 369, queue: true });
        $(".MainDiv_CenterDiv_StartInstall_IMG[iteminf*='3']").fadeOut({ duration: 369, queue: true });
        $(".MainDiv_CenterDiv_StartInstall_IMG[iteminf*='2']").fadeOut({ duration: 369, queue: true });
        $("#Label_InstallTitle").text(InstallTitle);
        $("#Label_InstallInfo").text(InstallInfo);
        $("#Label_InstallProcess").text("0%");
        $(`.MainDiv_CenterDiv_StartInstall_IMG[iteminf='${ItmeInfoStartBy}1']`).fadeIn(369,
            function() {
                $(`.MainDiv_CenterDiv_StartInstall_IMG[iteminf='${ItmeInfoStartBy}2']`).fadeIn(369,
                    function() {
                        $(`.MainDiv_CenterDiv_StartInstall_IMG[iteminf='${ItmeInfoStartBy}3']`).fadeIn(369,
                            function() {
                                $(
                                        ".MainDiv_CenterDiv_StartInstall_Proccess, .MainDiv_CenterDiv_StartInstall_Infromation")
                                    .fadeIn({ duration: 369, queue: true });
                                if (State >= 4) {
                                    //alert("Finish");
                                }

                                if (FuncStart != null) {
                                    FuncStart();
                                }
                            });
                    });
            });
    }

    // تابع برای تغییر عدد پردازش شده
    var ProccessInterval = null;

    function SetInstallProcess(number0To100, Duration, FuncIfFinish) {
        if (number0To100 == 1 && parseFloat($("#Label_InstallProcess").text()) <= 39) {

            number0To100 = RoundDecimalNumber((parseFloat($("#Label_InstallProcess").text()) + 0.05));
            $("#Label_InstallProcess").text(number0To100 + "%");
            return;
        } else if (number0To100 == 1 && parseFloat($("#Label_InstallProcess").text()) < 69) {
            number0To100 = RoundDecimalNumber((parseFloat($("#Label_InstallProcess").text()) + 0.005));
            $("#Label_InstallProcess").text(number0To100 + "%");
            return;
        } else if (number0To100 == 1 && parseFloat($("#Label_InstallProcess").text()) >= 69) {
            return;
        } else if (number0To100 <= parseFloat($("#Label_InstallProcess").text())) {
            return false;
        }

        if (ProccessInterval != null) {
            clearInterval(ProccessInterval);
            ProccessInterval = null;
        }

        const FlagRepeat = Duration / 100;
        const ChangeNumber = number0To100 - parseFloat($("#Label_InstallProcess").text());
        var ChangeFlag = ChangeNumber / FlagRepeat;

        ProccessInterval = setInterval(
            function() {
                if ((parseFloat($("#Label_InstallProcess").text()) + ChangeFlag) >= number0To100 ||
                    ((parseFloat($("#Label_InstallProcess").text()) + ChangeFlag) >= 99 && number0To100 == 100)) {
                    $("#Label_InstallProcess").text(number0To100 + "%");

                    if (FuncIfFinish != null && (parseFloat($("#Label_InstallProcess").text()) + ChangeFlag) >= 100) {
                        FuncIfFinish();
                    }

                    clearInterval(ProccessInterval);
                } else {
                    $("#Label_InstallProcess")
                        .text(RoundDecimalNumber((parseFloat($("#Label_InstallProcess").text()) + ChangeFlag)) + "%");
                }
            },
            100);
    }

    // تابع برای رند کردن اعشار - نمایش فقط با یک رقم اعشار
    function RoundDecimalNumber(Number) {
        const n = (Number + "").indexOf(".");
        if (n == -1) {
            return Number;
        } else {
            return (Number + "").substr(0, n + 2);
        }
    }


    // تابع برای نمایش دایو تعداد آیتم های مورد نظر برای جا به جایی
    function ShowItemCount(CountOfItem) {
        $("#MainDiv_CenterDiv_ItemCount_ChildDiv_CountOfItem").text(commaSeparateNumber(CountOfItem));
        $(".MainDiv_CenterDiv_ItemCount_ChildDiv").fadeIn(0);
        $(".MainDiv_CenterDiv_ItemCount_ChildDiv").animate({ "height": "69px" }, { duration: 963, queue: true });
        $(".MainDiv_CenterDiv_ItemCount_ChildDiv").children().fadeIn(639);
    }

    // تابع برای جاگذاری ، بین هر 3 کاراکتر
    function commaSeparateNumber(val) {
        val = val.toString();
        while (/(\d+)(\d{3})/.test(val)) {
            val = val.replace(/(\d+)(\d{3})/, "$1" + "," + "$2");
        }
        return val;
    }

    // تابع برای مخفی کردن دایو تعداد آیتم های مورد نظر برای جا به جایی
    function HideItemCount() {
        $(".MainDiv_CenterDiv_ItemCount_ChildDiv").children().fadeOut({ duration: 639, queue: true });
        $(".MainDiv_CenterDiv_ItemCount_ChildDiv").animate({ "height": "0px" }, { duration: 963, queue: true });
        $(".MainDiv_CenterDiv_ItemCount_ChildDiv").fadeOut({ duration: 0, queue: true });
    }

    // ##################################################### هندلر قسمت پنجم نصب برنامه #####################################################


    // ##################################################### هندلر قسمت ششم پایان نصب برنامه #####################################################

    // تابع برای نمایش قسمت پایانی نصب
    function ShowFinishedProgram() {
        HideItemCount();
        clearInterval(TimerInterval);
        StartLoading(function() {
            setTimeout(function() {
                    StopLoading(function() {
                        if (LastItemReceive.NumberOfErrorScript == 0) {
                            $("#MainDiv_CenterDiv_FinishInstall_ScriptErrorCount").text("Without problem");
                            $(".MainDiv_CenterDiv_FinishInstall_LineColor, #MainDiv_CenterDiv_FinishInstall_BtnError")
                                .css({ "background-color": "#006400" });
                        } else {
                            $("#MainDiv_CenterDiv_FinishInstall_ScriptErrorCount")
                                .text(LastItemReceive.NumberOfErrorScript);
                            $(".MainDiv_CenterDiv_FinishInstall_LineColor, #MainDiv_CenterDiv_FinishInstall_BtnError")
                                .css({ "background-color": "#ff8c00" });
                        }
                        $(".MainDiv_CenterDiv_FinishInstall").fadeIn(963);
                    });
                },
                3963);
        });
    }

    // ##################################################### هندلر قسمت ششم پایان نصب برنامه #####################################################


    // ##################################################### هندلر قسمت لودینگ #####################################################

    // تابع برای شروع نمایش صفحه لودینگ
    function StartLoading(funcIfNeed) {

        $(".Pages").fadeOut({ duration: 99, queue: true });
        $(".MainDiv_CenterDiv_Loading").fadeIn({ duration: 369, queue: true });

        if (funcIfNeed != null) {
            funcIfNeed();
        }

    }

    // تابع برای پایان نمایش لودینگ
    function StopLoading(funcIfNeed) {
        $(".MainDiv_CenterDiv_Loading").fadeOut({ duration: 369, queue: true });
        if (funcIfNeed != null) {
            funcIfNeed();
        }
    }


    // ##################################################### هندلر قسمت لودینگ #####################################################


    // ##################################################### هندلر قسمت ایجکس برای ارتباط با سایت #####################################################

    // تابع برای ارتباط به صورت ایجکس با سایت
    function SendAndReceiveDataAjax(DataForSend, funcSuccessReceive, funcError) {

        if (DataForSend == null) {
            $.ajax({
                url: "Home/G9DatabaseVersionControlHandler",
                type: "POST",
                dataType: "text",
                success: function(result) {

                    if (result.NeedShowMessage) {
                        ShowMessage(result.Message,
                            result.ShowAcceptAndDeniedBTN,
                            !result.Success);
                    }

                    funcSuccessReceive(result.d);
                },
                error: function(result) {
                    funcError(result);
                }
            });
        } else {
            $.ajax({
                url: "{{G9AjaxMethod}}",
                data: { data: JSON.stringify(DataForSend) },
                type: "POST",
                dataType: "text",
                success: function(result) {
                    result = JSON.parse(result);
                    if (result.NeedShowMessage) {
                        ShowMessage(result.Message,
                            result.ShowAcceptAndDeniedBTN,
                            !result.Success,
                            StopLoading(function() {
                                // Ignore
                            }));
                    }
                    funcSuccessReceive(result);
                },
                error: function(result) {
                    funcError(result);
                }
            });
        }

    }

    function ReadySendPacket(stepNumber, data) {
        return { TaskRequest: stepNumber, JsonData: data };
    }

});