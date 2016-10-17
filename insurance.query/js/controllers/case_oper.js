angular.module('app')
    .controller('case_oper_ctrl', [
        '$scope', '$http', '$uibModal', function ($scope, $http, $uibModal) {
            $scope.page.load_data = function () {
                var url = 'index/search_case?page_index=' + $scope.page.index + '&page_size=' + $scope.page.size;

                if ($scope._case.search.condition.id_card) {
                    url += '&id_card=' + $scope._case.search.condition.id_card;
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
                        id_card: undefined,
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

                    $http.post('index/save_case?case_str=' + angular.toJson(_case) + '&is_submit=' + is_submit).then(function (response) {
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

                    $http.get('index/untread_case?id=' + $scope._case.search.selected.ID).then(function (response) {
                        if (Number(response.data) > 0) {
                            $scope._case.search.from_server();
                            msg('回退成功！');
                        }
                    });
                },
                modify: function (size) {
                    $uibModal.open({
                        animation: true,
                        templateUrl: 'modify_case.html',
                        controller: function ($scope, $uibModalInstance, _case) {
                            $scope.selected = _case.selected;
                            $scope.save = function (is_submit) {
                                if (Number(_case.selected.SUBTRACTAMOUNT) > 0 && (typeof _case.selected.SUBTRACTREASON != "string"
                                    || _case.selected.SUBTRACTREASON.trim().length == 0)) {
                                    msg('有核减金额时必须填写核减原因');
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
                substract: function () {
                    $uibModal.open({
                        animation: true,
                        templateUrl: 'substract.html',
                        size: 'md',
                        controller: function ($scope, $uibModalInstance, search) {
                            $scope._case = {
                                search: {
                                    condition: {
                                        id_card: undefined,
                                        leave_date: undefined
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
                                    },
                                    from_server: function () {
                                        var params = undefined;
                                        _this = this;

                                        if (_this.condition.id_card) {
                                            params = 'id_card=' + _this.condition.id_card;
                                        } else if (_this.condition.leave_date) {
                                            params = 'leave_date=' + tools.get_today(0, _this.condition.leave_date);
                                        } else {
                                            msg('请输入身份证号码或出院日期');
                                        }

                                        if (params) {
                                            $http.get('index/search_patient?' + params).then(function (response) {
                                                if ($.isArray(response.data)) {
                                                    $.each(response.data, function (i) {
                                                        this.ID = i;
                                                    });
                                                    _this.result = response.data;
                                                    _this.selected = response.data.last();
                                                }
                                            });
                                        }
                                    }
                                },
                                save: function (is_submit) {
                                    if (Number(this.search.selected.SUBTRACTAMOUNT) > 0 && (typeof this.search.selected.SUBTRACTREASON != "string"
                                        || this.search.selected.SUBTRACTREASON.trim().length == 0)) {
                                        msg('有核减金额时必须填写核减原因');
                                        return;
                                    }

                                    if (is_submit && !confirm('确定要保存并提交吗？')) {  //msg('保存并提交成功') //msg('保存成功！');
                                        return;
                                    }

                                    var _case = angular.copy(this.search.selected);
                                    _case.ID = 0;
                                    _case.ADState = 0;

                                    is_submit = is_submit ? true : false;
                                    if (is_submit) {
                                        switch (parseInt($scope.user.authority_level)) {   //新增的权限逻辑写在前台，修改写在后台
                                            case 1:
                                                _case.TRIALER = $scope.user.name;
                                                _case.TRIALDATE = tools.get_today();
                                                _case.AUDITSTATUS = 9;
                                                _case.ADState = 1;
                                                break;
                                            case 9:
                                                _case.REVIEWER = $scope.user.name;
                                                _case.REVIEWDATE = tools.get_today();
                                                _case.AUDITSTATUS = 10;
                                                _case.ADState = 3;
                                                break;
                                        }
                                    }

                                    $http.post('index/save_case?case_str=' + angular.toJson(_case) + '&is_submit=' + is_submit).then(function (response) {
                                        if (Number(response.data) > 0) {
                                            _case.ID = response.data;

                                            if (!is_submit) {
                                                search.from_server();
                                            }

                                            if ($scope._case.search.result && $scope._case.search.result.length == 1) {
                                                $uibModalInstance.dismiss();
                                            }
                                            msg('保存成功！');
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

                            $scope.open = function () {
                                $scope.popup.opened = true;
                            };

                            $scope.setDate = function (year, month, day) {
                                $scope.dt = new Date(year, month, day);
                            };

                            $scope.formats = ['dd-MMMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
                            $scope.format = $scope.formats[0];
                            $scope.altInputFormats = ['M!/d!/yyyy'];

                            $scope.popup = {
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
                            search: $scope._case.search
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