mergeInto(LibraryManager.library, {

  // Create a new function with the same name as
  // the event listeners name and make sure the
  // parameters match as well.

  Log: function(msg) {

    // Within the function we're going to trigger
    // the event within the ReactUnityWebGL object
    // which is exposed by the library to the window.

    ReactUnityWebGL.Log(msg);
  },

  // Game loaded event
  Loaded: function() {
    ReactUnityWebGL.Loaded();
  }, 

  // Game start event
  GameStart: function() {
    ReactUnityWebGL.Start();
  },

  // Send level data in json format to react
  SaveLevelData: function(jsonString) {
    ReactUnityWebGL.SaveLevelData(UTF8ToString(jsonString));
  },

  // On level success or fail
  GameOver: function(jsonString) {
    ReactUnityWebGL.GameOver(UTF8ToString(jsonString));
  },

  // On log from robobot-game-server
  ConsoleLog: function(string) {
    ReactUnityWebGL.ConsoleLog(UTF8ToString(string));
  }

});
