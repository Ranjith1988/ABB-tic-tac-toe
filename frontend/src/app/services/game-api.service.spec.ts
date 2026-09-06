import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { GameApiService } from './game-api.service';
import { GameMode, GameState } from '../models/game.models';

describe('GameApiService', () => {
  let service: GameApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [GameApiService, provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(GameApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('creates a game through the REST API', () => {
    const expected = { gameId: 'game-1', mode: 'TwoPlayer' } as unknown as GameState;
    service.createGame('TwoPlayer').subscribe(result => expect(result).toEqual(expected));

    const request = httpMock.expectOne(request => request.url.endsWith('/games'));
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ mode: 'TwoPlayer' satisfies GameMode });
    request.flush(expected);
  });

  it('submits a move with player and coordinates', () => {
    service.makeMove('game-1', 'X', 2, 1).subscribe();

    const request = httpMock.expectOne(request => request.url.endsWith('/games/game-1/moves'));
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ player: 'X', row: 2, column: 1 });
    request.flush({});
  });

  it('maps backend errors to a user-friendly error', () => {
    let errorMessage = '';
    service.getGame('missing').subscribe({ error: error => errorMessage = error.message });

    const request = httpMock.expectOne(request => request.url.endsWith('/games/missing'));
    request.flush({ code: 'GAME_NOT_FOUND', message: 'Game was not found.' }, { status: 400, statusText: 'Bad Request' });
    expect(errorMessage).toBe('Game was not found.');
  });
});
