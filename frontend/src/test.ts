import { getTestBed } from '@angular/core/testing';
import { BrowserDynamicTestingModule, platformBrowserDynamicTesting } from '@angular/platform-browser-dynamic/testing';
import 'zone.js/testing';

getTestBed().initTestEnvironment(BrowserDynamicTestingModule, platformBrowserDynamicTesting());

import './app/app.component.spec';
import './app/services/game-api.service.spec';
import './app/components/game-board/game-board.component.spec';
import './app/components/move-history/move-history.component.spec';
import './app/components/scoreboard/scoreboard.component.spec';
