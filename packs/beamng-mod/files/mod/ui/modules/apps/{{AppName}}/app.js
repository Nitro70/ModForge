// {{ModName}} {{Version}} by {{Author}}
//
// Three names must agree or the app is listed but stays empty:
//   the directive name below      = "directive" in app.json   ({{AppDirective}})
//   the element in "domElement"   = that name in dash form    (<{{AppElement}}>)
//   the folder name               = the path in templateUrl   ({{AppName}})
angular.module('beamng.apps')
.directive('{{AppDirective}}', [function () {
  return {
    templateUrl: '/ui/modules/apps/{{AppName}}/app.html',
    replace: true,
    restrict: 'EA',
    scope: true,
    link: function (scope, element, attrs) {
      // Ask the player's vehicle to send its electrics values to the UI.
      var streamsList = ['electrics']
      StreamsManager.add(streamsList)
      scope.$on('$destroy', function () {
        StreamsManager.remove(streamsList)
      })

      scope.title = '{{ModName}}'
      scope.speed = '---'

      // Fires many times a second while the player is in a vehicle.
      scope.$on('streamsUpdate', function (event, streams) {
        if (!streams.electrics) { return }
        scope.$evalAsync(function () {
          // wheelspeed is in metres per second. UiUnits follows the player's unit setting.
          scope.speed = UiUnits.buildString('speed', streams.electrics.wheelspeed || 0, 0)
        })
      })
    }
  }
}])
