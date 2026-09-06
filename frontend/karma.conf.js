module.exports = function (config) {
  config.set({
    basePath: '',
    frameworks: ['jasmine'],
    plugins: [
      require('karma-jasmine'),
      require('karma-chrome-launcher'),
      require('karma-jasmine-html-reporter'),
      require('karma-coverage')
    ],
    client: { jasmine: { random: false } },
    reporters: ['progress', 'kjhtml'],
    coverageReporter: { dir: require('path').join(__dirname, './coverage/tic-tac-toe-frontend') },
    browsers: ['ChromeHeadless'],
    restartOnFileChange: true,
    singleRun: true
  });
};
