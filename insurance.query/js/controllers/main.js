angular.module('app')
    .controller('main_ctrl', [
        '$scope', '$http', function($scope, $http) {
            $scope.users = {};
            $scope.users.action = {
                modify: function() {
//                    $http.get('index/get_user?name=' + $scope.user.name + '&authority_level=' + $scope.user.authority_level).then(function(response) {
//                        if (!$.isEmptyObject(response.data)) {
//                            $scope.dialog = {
//                                state: 'modify',
//                                title: "修改用户信息",
//                                user: response.data
//                            };
//                            $('#user_modal').modal('show');
//                        } else {
//                            msg('未找到该用户信息，请重新登录');
//                        }
                    //                    });
                    msg('该功能正在开发中......');
                },
                save: function() {
                    var params = {
                        uid: $scope.dialog.user.id,
                        user_name: $scope.dialog.user.name.trim(),
                        password: $scope.dialog.user.password.trim(),
                        authority_level: $scope.dialog.user.authority_level
                    };
                    $http.get('index/modify_user?user_info=' + angular.toJson(params)).then(function(response) {
                        if (response.data > 0) {
                            msg('修改成功！');
                            $('#user_modal').modal('hide');
                            $('.modal-backdrop').fadeOut(800).remove();
                        } else {
                            msg(response.data);
                            throw response.data;
                        }
                    });
                }
            };

            $scope.page = {
                inited: false,  //是否已初始化
                index: 0,   //页号,表示第一页
                filtered: [1],  //当前显示的页集合
                all_items: [],  //所有的页号
                size: 15,   //每页显示多少条记录
                per_num: 5,  //页集合的页号数量
                offset_num: 3,  //页号(左、右)偏移量
                record_count: 0,    //总记录数
                turn_to: function(index) {
                    if (index < 0) {
                        msg('已经是第一页');
                    } else if (index > (this.all_items.length - 1)) {
                        msg('已经是最后一页');
                    } else if ($scope.page.all_items.length < $scope.page.per_num && (index == 0 || index == $scope.page.all_items.last() - 1)){  //如果总页数少于页集合的页号数量,即看得到首页和末页,则不需要分页
                        this.index = index;
                        this.load_data();
                    } else {
                        var i = 0, start_index = 0, end_index = 0;
                        angular.forEach($scope.page.filtered, function(val, j) {
                            if (val == (index + 1)) {
                                i = j;
                                return false;
                            }
                        });

                        if (i == 0 || i + 1 == $scope.page.filtered.length) { //the first one
                            $.each($scope.page.all_items, function(i, val) {
                                if (val == (index + 1)) {
                                    return false;
                                }
                            });

                            if (i == 0) {
                                start_index = index - $scope.page.offset_num;
                            } else {
                                start_index = index + $scope.page.offset_num - ($scope.page.per_num - 1);
                            }
                            start_index = start_index >= 0 ? start_index : 0;

                            end_index = start_index + $scope.page.per_num;
                            if (end_index + 1 >= $scope.page.all_items.length) {
                                start_index = $scope.page.all_items.length - $scope.page.per_num;
                                end_index = $scope.page.all_items.length;
                            }

                            $scope.page.filtered = $scope.page.all_items.slice(start_index, end_index);
                        }

                        this.index = index;
                        this.load_data();
                    }
                },
                load_data: function() {
                    //默认的方法 for case search
                }
            };
        }
    ]);