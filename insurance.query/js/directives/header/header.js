angular.module('app')
    .directive('header', function () {
        return {
            templateUrl: 'js/directives/header/header.html',
            restrict: 'E',
            replace: true
        }
    })
    .directive('headerNotification', function () {
        return {
            templateUrl: 'js/directives/header/header-notification.html',
            restrict: 'E',
            replace: true
        }
    });