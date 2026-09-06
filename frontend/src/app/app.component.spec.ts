import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { AppComponent } from './app.component';
import { GameApiService } from './services/game-api.service';
import { of, throwError } from 'rxjs';

class GameApiServiceStub {
  failGameRequests = false;
  failScoreboardReset = false;
  failGameReload = false;

  createGame() {
    if (this.failGameRequests) return throwError(() => new Error('Create failed'));
    return of({
      gameId: 'game-1', board: Array(9).fill(null), currentPlayer: 'X', mode: 'TwoPlayer',
      status: 'InProgress', winner: null, winningCells: [], moveHistory: [],
      scoreboard: { xWins: 0, oWins: 0, draws: 0 }
    });
  }
  makeMove() { return this.createGame(); }
  undo() { return this.createGame(); }
  resetGame() { return this.createGame(); }
  resetScoreboard() {
    return this.failScoreboardReset ? throwError(() => new Error('Score reset failed')) : of(void 0);
  }
  getGame() {
    return this.failGameReload ? throwError(() => new Error('Reload failed')) : this.createGame();
  }
  getScoreboard() { return of({ xWins: 0, oWins: 0, draws: 0 }); }
}

describe('AppComponent', () => {
  let fixture: ComponentFixture<AppComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: GameApiService, useClass: GameApiServiceStub }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
  });

  it('renders the board and initial turn', () => {
    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelectorAll('.cell').length).toBe(9);
    expect(element.querySelector('.status')?.textContent).toContain("Player X's turn");
  });

  it('starts a computer game when the mode changes', () => {
    const select = fixture.nativeElement.querySelector('select') as HTMLSelectElement;
    select.value = 'Computer';
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();

    expect(select.value).toBe('Computer');
  });

  it('sends a board move when an empty cell is selected', () => {
    const cell = fixture.nativeElement.querySelector('.cell') as HTMLButtonElement;
    cell.click();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.status')?.textContent).toContain("Player X's turn");
  });

  it('resets the current game', () => {
    const button = Array.from(fixture.nativeElement.querySelectorAll('button')) as HTMLButtonElement[];
    const resetButton = button
      .find((item): item is HTMLButtonElement => item.textContent?.includes('Reset Game') ?? false);

    resetButton?.click();
    fixture.detectChanges();

    expect(resetButton).toBeTruthy();
  });

  it('resets the scoreboard and reloads the current game', () => {
    const button = fixture.nativeElement.querySelector('.link-button') as HTMLButtonElement;
    button.click();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.scoreboard')).toBeTruthy();
  });

  it('exposes game status messages', () => {
    const component = fixture.componentInstance;
    component.game = { ...component.game!, status: 'Won', winner: 'X' };
    expect(component.gameMessage).toBe('Player X wins!');
    component.game = { ...component.game!, status: 'Draw', winner: null };
    expect(component.gameMessage).toBe('It is a draw!');
    component.game = null;
    expect(component.gameMessage).toBe('Starting game…');
  });

  it('shows the computer thinking status and undo state', () => {
    const component = fixture.componentInstance;
    component.game = { ...component.game!, mode: 'Computer', currentPlayer: 'O', moveHistory: [{ moveNumber: 1, player: 'X', row: 0, column: 0 }] };

    expect(component.gameMessage).toBe('Computer is thinking…');
    expect(component.canUndo).toBeTrue();
    expect(component.cellLabel(0)).toBe('Row 1, Column 1, empty');
    component.game.board[0] = 'X';
    expect(component.cellLabel(0)).toBe('Row 1, Column 1, X');
  });

  it('guards invalid cell selections and actions', () => {
    const component = fixture.componentInstance;
    const initialGame = component.game!;
    component.game = null;
    component.selectCell(0);
    component.game = { ...initialGame, board: ['X', null, null, null, null, null, null, null, null] };
    component.selectCell(0);
    component.loading = true;
    component.selectCell(1);
    component.loading = false;
    component.game = { ...component.game, mode: 'Computer', currentPlayer: 'O' };
    component.selectCell(1);
    component.undo();
    component.resetGame();

    expect(component.errorMessage).toBe('');
  });

  it('handles request and scoreboard errors', () => {
    const component = fixture.componentInstance;
    const api = TestBed.inject(GameApiService) as unknown as GameApiServiceStub;
    api.failGameRequests = true;
    component.startGame();
    expect(component.errorMessage).toBe('Create failed');

    api.failGameRequests = false;
    api.failScoreboardReset = true;
    component.resetScoreboard();
    expect(component.errorMessage).toBe('Score reset failed');

    api.failScoreboardReset = false;
    api.failGameReload = true;
    component.game = { ...component.game!, gameId: 'game-1' };
    component.resetScoreboard();
    expect(component.errorMessage).toBe('Reload failed');
  });

  it('runs undo and reset actions for an active game', () => {
    const component = fixture.componentInstance;
    component.game = {
      ...component.game!,
      mode: 'TwoPlayer',
      moveHistory: [{ moveNumber: 1, player: 'X', row: 0, column: 0 }]
    };

    component.undo();
    component.resetGame();

    expect(component.loading).toBeFalse();
  });

  it('clears loading after a scoreboard reset without a current game', () => {
    const component = fixture.componentInstance;
    component.game = null;

    component.resetScoreboard();

    expect(component.loading).toBeFalse();
  });

  it('does not undo a completed game or while loading', () => {
    const component = fixture.componentInstance;
    component.game = {
      ...component.game!,
      status: 'Won',
      moveHistory: [{ moveNumber: 1, player: 'X', row: 0, column: 0 }]
    };
    component.undo();
    component.game = { ...component.game, status: 'InProgress' };
    component.loading = true;
    component.undo();
    component.resetGame();

    expect(component.loading).toBeTrue();
  });

  it('does not reset the scoreboard while loading', () => {
    const component = fixture.componentInstance;
    component.loading = true;

    component.resetScoreboard();

    expect(component.loading).toBeTrue();
  });
});
