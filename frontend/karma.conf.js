module.exports = function (config) {
  config.set({
    basePath: '',
    frameworks: ['jasmine', '@angular-devkit/build-angular'],
    plugins: [
      require('karma-jasmine'),
      require('karma-chrome-launcher'),
      require('karma-jasmine-html-reporter'),
      require('karma-coverage'),
      require('@angular-devkit/build-angular/plugins/karma')
    ],
    client: { jasmine: { random: false } },
    reporters: ['progress', 'kjhtml'],
    coverageReporter: {
      dir: require('path').join(__dirname, './coverage/tic-tac-toe-frontend'),
      reporters: [{ type: 'html' }, { type: 'text-summary' }],
      check: {
        global: { statements: 100, branches: 100, functions: 100, lines: 100 }
      }
    },
    browsers: ['ChromeHeadless'],
    restartOnFileChange: true,
    singleRun: true
  });
};
