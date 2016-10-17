angular
    .module('app', [
        'angular-loading-bar',
        'oc.lazyLoad',
        'ui.router',
        'ngAnimate',
        'ui.bootstrap'
    ])
    .config([
        '$stateProvider', '$urlRouterProvider', '$ocLazyLoadProvider',
        function ($stateProvider, $urlRouterProvider, $ocLazyLoadProvider) {

            $ocLazyLoadProvider.config({
                debug: false,
                events: true
            });

            $urlRouterProvider.when('', '/login');
            $urlRouterProvider.otherwise('/policy/search');

            var resolve_dep = function (config) {
                return {
                    load: function ($ocLazyLoad) {
                        return $ocLazyLoad.load(config);
                    }
                };
            };

            $stateProvider
                .state('main', {
                    url: '/main',
                    controller: 'main_ctrl',
                    templateUrl: 'templates/main.html',
                    resolve: resolve_dep([
                        'js/controllers/main.js',
                        'js/directives/header/header.js',
                        'js/directives/sidebar/sidebar.js'
                    ])
                })
                .state('main.case_oper', {
                    url: '/case_oper',
                    controller: 'case_oper_ctrl',
                    templateUrl: 'templates/case_oper.html',
                    resolve: resolve_dep('js/controllers/case_oper.js')
                })
                .state('main.monthly_statement', {
                    url: '/monthly_statement',
                    controller: 'monthly_statement_ctrl',
                    templateUrl: 'templates/monthly_statement.html',
                    resolve: resolve_dep( 'js/controllers/monthly_statement.js')
                })
                .state('main.summary_list', {
                    url: '/summary_list',
                    controller: 'summary_list_ctrl',
                    templateUrl: 'templates/summary_list.html',
                    resolve: resolve_dep('js/controllers/summary_list.js')
                })
                .state('main.illness_payment', {
                    url: '/illness_payment',
                    controller: 'illness_payment_ctrl',
                    templateUrl: 'templates/illness_payment.html',
                    resolve: resolve_dep('js/controllers/illness_payment.js')
                })
                .state('main.personnel_info', {
                    url: '/personnel_info',
                    controller: 'personnel_info_ctrl',
                    templateUrl: 'templates/personnel_info.html',
                    resolve: resolve_dep('js/controllers/personnel_info.js')
                })
                .state('login', {
                    templateUrl: 'templates/login.html',
                    url: '/login',
                    resolve: resolve_dep('js/directives/login.js')
                });
        }
    ]);