angular.module('app')
    .directive('userLogin', [
        '$rootScope', '$http', function ($r_scope, $http) {
            return function () {
                $(function () {
                    $("#login").click(function () {
                        var username = $("#username").val();
                        var password = $("#password").val();
                        var $msg = $("#message");
                        if (typeof (username) != "string" || username.trim().length == 0) {
                            $("#username").focus();
                            $msg.html("请输入输入用户名").addClass("text-danger");
                        } else if (typeof (password) != "string" || password.trim().length == 0) {
                            $("#password").focus();
                            $msg.html("请输入输入密码").addClass("text-danger");
                        } else {
                            username = username.trim();
                            password = password.trim()

                            $http.get('index/validate_user?username=' + username + '&password=' + password + '&t=' + Math.random()).then(function (response) {
                                if (angular.isObject(response.data)){
                                    $r_scope.user = {
                                        name: username,
                                        authority_level: response.data.authority_level
                                    };

                                    $.cookie('username', $r_scope.user.name);
                                    $.cookie('authority_level', $r_scope.user.authority_level);

                                    if ($r_scope.user.authority_level == 1 || $r_scope.user.authority_level == 9) {
                                        location.href = "#/main/case_oper";
                                    } else {
                                        location.href = "#/main/monthly_statement";
                                    }
                                } else if (response.data == 0) {
                                    $msg.html("用户名或密码不正确").addClass("text-danger");
                                } else {
                                    $msg.html(response.data).addClass("text-danger");
                                }
                            });
                        }
                    });

                    $('.login-panel').fadeIn(2000);
                });
            };
        }
    ]);