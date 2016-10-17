angular.module('app')
    .controller('monthly_statement_ctrl', [
        '$scope', '$http', '$uibModal', function ($scope, $http, $uibModal) {
            $scope.page.load_data = function () {
                var url = 'index/search_monthly_data?page_index=' + $scope.page.index + '&page_size=' + $scope.page.size;

                if ($scope._case.search.condition.yjlsh) {
                    url += '&yjlsh=' + $scope._case.search.condition.yjlsh;
                }

                if ($scope._case.search.condition.is_all) {
                    url += '&is_all=true';
                } else {
                    url += '&is_all=false';
                }

                $http.get(url).then(function (response) {
                    if (angular.isObject(response.data)) {
                        $scope._case.search.result = response.data.cases;
                        $scope._case.search.selected = response.data.cases.last();

                        if (response.data.page_count > 0) {
                            $scope.page.all_items = [];
                            for (var i = 0; i < response.data.page_count; i++) {
                                $scope.page.all_items.push(i + 1);
                            }

                            $scope.page.record_count = response.data.record_count;
                            if (!$scope.page.inited) {
                                if ($scope.page.all_items.length >= $scope.page.per_num) {
                                    $scope.page.filtered = $scope.page.all_items.slice(0, $scope.page.per_num);
                                } else {
                                    $scope.page.filtered = $scope.page.all_items;
                                }
                                $scope.page.inited = true;
                            }
                        }
                    }
                });
            };

            $scope._case = {
                search: {
                    condition: {
                        yjlsh: undefined,
                        is_all: false
                    },
                    result: [],
                    selected: undefined,
                    select: function (_this) {
                        $.each($scope._case.search.result, function () {
                            if (this.ID == _this.ID) {
                                this.checked = true;
                            } else {
                                this.checked = false;
                            }
                        });
                        this.selected = _this;
                    },
                    from_server: function () {
                        $scope.page.inited = false;
                        $scope.page.index = 0;
                        $scope.page.load_data();
                    }
                },
                save: function (_case, is_submit, callback) {
                    if (is_submit && !confirm('确定要提交吗？')) {  //msg('保存并提交成功') //msg('保存成功！');
                        return;
                    }

                    _case = angular.isObject(_case) ? _case : $scope._case.search.selected;
                    is_submit = is_submit ? true : false;

                    $http.post('index/save_monthly_data?tb0016=' + angular.toJson(_case) + '&is_submit=' + is_submit).then(function (response) {
                        if (Number(response.data) > 0) {
                            if ($.isFunction(callback)) { //修改
                                callback();
                            }

                            if (is_submit) {
                                $scope._case.search.from_server();

                                if ($.isFunction(callback)) {
                                    msg('保存并提交成功！');
                                } else {
                                    msg('提交成功！');
                                }
                            } else {
                                $.each($scope._case.search.result, function (i) {
                                    if (this.ID == _case.ID) {
                                        $scope._case.search.result[i] = _case;
                                        $scope._case.search.result.selected = _case;

                                        return false;
                                    }
                                });

                                msg('保存成功！');
                            }
                        }
                    });
                },
                untread: function () {
                    if (!confirm('确定要回退吗？')) {  //msg('保存并提交成功') //msg('保存成功！');
                        return;
                    }

                    $http.get('index/untread_monthly_data?id=' + $scope._case.search.selected.ID).then(function (response) {
                        if (Number(response.data) > 0) {
                            $scope._case.search.from_server();
                            msg('回退成功！');
                        }
                    });
                },
                modify_monthly_data: function (size) {
                    $uibModal.open({
                        animation: true,
                        templateUrl: 'modify_monthly_data.html',
                        controller: function ($scope, $uibModalInstance, _case) {
                            $scope.selected = _case.selected;
                            $scope.save = function (is_submit) {
                                if (Number(_case.selected.AKB144) > 0 && (typeof _case.selected.AKB147 != "string"
                                        || _case.selected.AKB147.trim().length == 0)) {
                                    msg('有扣付金额时必须填写扣付原因');
                                    return;
                                }

                                _case.save($scope.selected, is_submit, $uibModalInstance.dismiss);
                            }
                        },
                        resolve: {
                            _case: {
                                selected: angular.copy($scope._case.search.selected),
                                save: $scope._case.save
                            }
                        }
                    });
                },
                edit_monthly_data: function () {
                    $uibModal.open({
                        animation: true,
                        templateUrl: 'edit_monthly_data.html',
                        size: 'lg',
                        controller: function ($scope, $uibModalInstance, scope) {
                            $scope._case = {
                                search: {
                                    condition: {
                                        start_time: undefined,
                                        end_time: undefined
                                    },
                                    result: undefined,
                                    selected: undefined,
                                    select: function (_this) {
                                        $.each($scope._case.search.result, function () {
                                            if (this.ID == _this.ID) {
                                                this.checked = true;
                                            } else {
                                                this.checked = false;
                                            }
                                        });
                                        this.selected = _this;

                                        //调取该时间段范围内的核减金额，和核减原因数据
                                        $http.get('index/count_substract_info?start_time=' + _this.AAE030 + '&end_time=' + _this.AAE031
                                            + '&t=' + Math.random()).then(function (response) {
                                                if (angular.isObject(response.data)) {
                                                    _this.AKB144 = response.data.substract_amount;
                                                    _this.AKB147 = response.data.substract_reason.replace_all('/n', '\n');
                                                }
                                            });
                                    },
                                    from_server: function () {
                                        _this = this;

                                        $http.get('index/edit_monthly_data?start_time=' + tools.get_today(0, _this.condition.start_time)
                                            + '&end_time=' + tools.get_today(0, _this.condition.end_time)).then(function (response) {
                                                if ($.isArray(response.data)) {
                                                    _this.result = response.data;
                                                }
                                            });
                                    }
                                },
                                save: function (is_submit) {
                                    if (is_submit && !confirm('确定要提交吗？')) {
                                        return;
                                    }

                                    this.search.selected.ID = 0;
                                    switch (Number(scope.user.authority_level)) {
                                        case 1:  //业务初审
                                            this.search.selected.AAE026 = scope.user.name;
                                            this.search.selected.AAE025 = tools.get_today();
                                            this.search.selected.AAE093 = undefined;
                                            this.search.selected.AAE092 = undefined;
                                            this.search.selected.AAE095 = undefined;
                                            this.search.selected.AAE094 = undefined;
                                            this.search.selected.AAE032 = undefined;

                                            if (is_submit) {
                                                this.search.selected.AAE117 = 9;
                                                this.search.selected.ADState = 1;
                                            } else {
                                                this.search.selected.AAE117 = 1;
                                                this.search.selected.ADState = 0;
                                            }
                                            break;
                                        case 9:  //业务复审
                                            this.search.selected.AAE093 = scope.user.name;
                                            this.search.selected.AAE092 = tools.get_today();
                                            this.search.selected.AAE095 = undefined;
                                            this.search.selected.AAE094 = undefined;
                                            this.search.selected.AAE032 = undefined;

                                            if (is_submit) {
                                                this.search.selected.AAE117 = 10;
                                                this.search.selected.ADState = 3;
                                            }
                                            break;
                                        case 10:  //财务审核
                                            this.search.selected.AAE095 = scope.user.name;
                                            this.search.selected.AAE094 = tools.get_today();
                                            this.search.selected.AAE032 = undefined;

                                            if (is_submit) {
                                                this.search.selected.AAE117 = 2;
                                                this.search.selected.ADState = 5;
                                            }
                                            break;
                                        case 2:  //领导审批
                                            this.search.selected.AAE032 = scope.user.name;

                                            if (is_submit) {
                                                this.search.selected.ADState = 7;
                                            }
                                            break;
                                    }

                                    is_submit = is_submit ? true : false;
                                    delete this.search.selected.EntityKey;
                                    delete this.search.selected.$id;
                                    $http.post('index/save_monthly_data?tb0016=' + angular.toJson(this.search.selected) + '&is_submit=' + is_submit).then(function (response) {
                                        if (Number(response.data) > 0) {
                                            $scope._case.search.selected.ID = response.data;
                                            scope.search.from_server();

                                            $uibModalInstance.dismiss();

                                            if (is_submit) {
                                                msg('保存并提交成功');
                                            } else {
                                                msg('保存成功');
                                            }
                                        }
                                    });
                                }
                            }
                            //---------------datapicker-----------------
                            $scope.today = function () {
                                $scope.dt = new Date();
                            };
                            $scope.today();

                            $scope.clear = function () {
                                $scope.dt = null;
                            };

                            $scope.inlineOptions = {
                                customClass: getDayClass,
                                minDate: new Date(),
                                showWeeks: true
                            };

                            $scope.dateOptions = {
                                dateDisabled: disabled,
                                formatYear: 'yy',
                                maxDate: new Date(2020, 5, 22),
                                minDate: new Date(),
                                startingDay: 1
                            };

                            // Disable weekend selection
                            function disabled(data) {
                                var date = data.date,
                                    mode = data.mode;
                                return mode === 'day' && (date.getDay() === 0 || date.getDay() === 6);
                            }

                            $scope.toggleMin = function () {
                                $scope.inlineOptions.minDate = $scope.inlineOptions.minDate ? null : new Date();
                                $scope.dateOptions.minDate = $scope.inlineOptions.minDate;
                            };

                            $scope.toggleMin();

                            $scope.open1 = function () {
                                $scope.popup1.opened = true;
                            };
                            $scope.open2 = function () {
                                $scope.popup2.opened = true;
                            };

                            $scope.setDate = function (year, month, day) {
                                $scope.dt = new Date(year, month, day);
                            };

                            $scope.formats = ['dd-MMMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
                            $scope.format = $scope.formats[0];
                            $scope.altInputFormats = ['M!/d!/yyyy'];

                            $scope.popup1 = {
                                opened: false
                            };
                            $scope.popup2 = {
                                opened: false
                            };

                            var tomorrow = new Date();
                            tomorrow.setDate(tomorrow.getDate() + 1);
                            var afterTomorrow = new Date();
                            afterTomorrow.setDate(tomorrow.getDate() + 1);
                            $scope.events = [
                                {
                                    date: tomorrow,
                                    status: 'full'
                                },
                                {
                                    date: afterTomorrow,
                                    status: 'partially'
                                }
                            ];

                            function getDayClass(data) {
                                var date = data.date,
                                    mode = data.mode;
                                if (mode === 'day') {
                                    var dayToCheck = new Date(date).setHours(0, 0, 0, 0);

                                    for (var i = 0; i < $scope.events.length; i++) {
                                        var currentDay = new Date($scope.events[i].date).setHours(0, 0, 0, 0);

                                        if (dayToCheck === currentDay) {
                                            return $scope.events[i].status;
                                        }
                                    }
                                }

                                return '';
                            }
                            //---------------datapicker-----------------
                        },
                        resolve: {
                            scope: {
                                user: $scope.user,
                                search: $scope._case.search
                            }
                        }
                    });
                },
                state_name: function (state) {
                    switch (parseInt(state)) {
                        case 0:
                            return '新建';
                        case 1:
                            return '初审提交';
                        case 2:
                            return '复审退回';
                        case 3:
                            return '复审提交';
                        case 4:
                            return '财务审核退回';
                        case 5:
                            return '财务审核提交';
                        case 6:
                            return '领导审批退回';
                        case 7:
                            return '领导审批提交';
                        default:
                            return '';
                    }
                }
            };

            $scope._case.search.from_server();

            window.$scope = $scope;
        }
]);